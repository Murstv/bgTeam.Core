﻿namespace bgTeam.Web.Impl
{
    using bgTeam;
    using bgTeam.Extensions;
    using bgTeam.Web;
    using bgTeam.Web.Builders;
    using bgTeam.Web.Exceptions;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using System.Web;

    public class WebClient : IWebClient
    {
        private readonly string _url;
        private readonly IAppLogger _logger;
        private readonly HttpClient _client;
        private readonly IContentBuilder _builder;

#if NETCOREAPP2_1 || NETCOREAPP2_2
        private readonly SocketsHttpHandler _handler;
#else
        private readonly ServicePoint _servicePoint;
#endif

        public WebClient(IAppLogger logger, string url)
            : this(logger, url, new FormUrlEncodedContentBuilder())
        {

        }

#if NETCOREAPP2_1 || NETCOREAPP2_2
        public WebClient(IAppLogger logger, string url, X509CertificateCollection clientCertificates)
            : this(logger, url, new FormUrlEncodedContentBuilder())
        {
            var sslOptions = new System.Net.Security.SslClientAuthenticationOptions();
            sslOptions.ClientCertificates = clientCertificates;
            sslOptions.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            _handler.SslOptions = sslOptions;
        }
#endif

        public WebClient(IAppLogger logger, string url, IContentBuilder builder)
        {
            _logger = logger;
            _url = url.CheckNull(nameof(url));
            _builder = builder.CheckNull(nameof(builder));

            var uri = new Uri(_url);
#if NETCOREAPP2_1 || NETCOREAPP2_2
            _handler = new SocketsHttpHandler();
            _client = new HttpClient(_handler);
#else
            _client = new HttpClient();
            _servicePoint = ServicePointManager.FindServicePoint(uri);
#endif
            _client.BaseAddress = uri;

            ConnectionsLimit = 1024;
            MaxIdleTime = 300000; // 5 мин
            ConnectionLeaseTimeout = 0; // закрываем соединение сразу после выполнения запроса
        }

        /// <summary>
        /// Количество одновременных запросов на удалённый сервер. По-умолчанию для .net core int.Max, в остальных случаях 2
        /// </summary>
        public int ConnectionsLimit
        {
            get
            {
#if NETCOREAPP2_1 || NETCOREAPP2_2
                return _handler.MaxConnectionsPerServer;
#else
                return ServicePointManager.DefaultConnectionLimit;
#endif
            }

            set
            {
#if NETCOREAPP2_1 || NETCOREAPP2_2
                _handler.MaxConnectionsPerServer = value;
#else
                ServicePointManager.DefaultConnectionLimit = value;
#endif
            }
        }

        /// <summary>
        /// Указывает, сколько времени (в мс) будет закеширован полученный IP адрес для каждого доменного имени
        /// </summary>
        /// <exception>
        /// NotSupportedException для сред .net core
        /// </exception>
        public int DnsRefreshTimeout
        {
            get
            {
#if NETCOREAPP2_1 || NETCOREAPP2_2
                throw new NotSupportedException();
#else
                return ServicePointManager.DnsRefreshTimeout;
#endif
            }

            set
            {
#if NETCOREAPP2_1 || NETCOREAPP2_2
                throw new NotSupportedException();
#else
                ServicePointManager.DnsRefreshTimeout = value;
#endif
            }
        }

        /// <summary>
        /// Указывает, после какого времени бездействия (в мс) соединение будет закрыто. Бездействие означает отсутствие передачи данных через соединение.
        /// </summary>
        /// <exception>
        /// NotSupportedException для среды .net core 2.0
        /// </exception>
        public int MaxIdleTime
        {
            get
            {
#if NETCOREAPP2_1 || NETCOREAPP2_2
                return Convert.ToInt32(_handler.PooledConnectionIdleTimeout.TotalMilliseconds);
#else
                return _servicePoint.MaxIdleTime;
#endif
            }

            set
            {
#if NETCOREAPP2_1 || NETCOREAPP2_2
                _handler.PooledConnectionIdleTimeout = TimeSpan.FromMilliseconds(value);
#else
                _servicePoint.MaxIdleTime = value;
#endif
            }
        }

        /// <summary>
        /// Указывает, сколько времени (в мс) соединение может удерживаться открытым. По умолчанию лимита времени жизни для соединений нет. Установка его в 0 приведет к тому, что каждое соединение будет закрываться сразу после выполнения запроса.
        /// </summary>
        /// <exception>
        /// NotSupportedException для среды .net core 2.0
        /// </exception>
        public int ConnectionLeaseTimeout
        {
            get
            {
#if NETCOREAPP2_1 || NETCOREAPP2_2
                return Convert.ToInt32(_handler.PooledConnectionLifetime.TotalMilliseconds);
#else
                return _servicePoint.ConnectionLeaseTimeout;
#endif
            }

            set
            {
#if NETCOREAPP2_1
                _handler.PooledConnectionLifetime = TimeSpan.FromMilliseconds(value);
#else
                _servicePoint.ConnectionLeaseTimeout = value;
#endif
            }
        }

        /// <summary>
        /// Устанавливает или возвращает таймаут запроса к серверу
        /// </summary>
        public TimeSpan RequestTimeout
        {
            get { return _client.Timeout; }
            set { _client.Timeout = value; }
        }

        public string Url => _url;

        public async Task<T> GetAsync<T>(string method, IDictionary<string, object> queryParams = null, IDictionary<string, object> headers = null)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(_url))
            {
                method = method.CheckNull(nameof(method));
            }

            string url = BuildGetUrl(method, queryParams);
            var msg = new HttpRequestMessage(HttpMethod.Get, url);
            FillHeaders(headers, msg);

            var resultGet = await _client.SendAsync(msg);
            return await ProcessResultAsync<T>(resultGet);
        }

        public async Task<T> PostAsync<T>(string method, object postParams = null, IDictionary<string, object> headers = null)
            where T : class
        {
            var result = string.Empty;

            HttpContent content = null;
            if (postParams != null)
            {
                content = _builder.Build(postParams);
            }

            if (!headers.NullOrEmpty())
            {
                if (content == null)
                {
                    content = new StringContent(string.Empty);
                }

                FillContentHeaders(headers, content);
            }

            var resultPost = await _client.PostAsync(method, content);
            return await ProcessResultAsync<T>(resultPost);
        }

        private void CheckResult(HttpResponseMessage resultPost)
        {
            switch (resultPost.StatusCode)
            {
                case HttpStatusCode.OK:
                case HttpStatusCode.NoContent:
                case HttpStatusCode.PartialContent:
                    {
                        return;
                    }

                case HttpStatusCode.BadRequest:
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.NotFound:
                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.BadGateway:
                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.GatewayTimeout:
                    {
                        throw new WebClientException($"Status code: {resultPost.StatusCode}. Message: {resultPost.ReasonPhrase}", resultPost.StatusCode);
                    }

                default:
                    {
                        throw new WebClientException($"Unknown http error. Status code: {resultPost.StatusCode}. Message {resultPost.ReasonPhrase}", resultPost.StatusCode);
                    }
            }
        }

        private void FillContentHeaders(IDictionary<string, object> headers, HttpContent content)
        {
            foreach (var header in headers)
            {
                content.Headers.Add(header.Key, header.Value.ToString());
            }
        }

        private string BuildGetUrl(string method, IDictionary<string, object> queryParams)
        {
            string baseUrl;
            if (!string.IsNullOrWhiteSpace(_url))
            {
                baseUrl = _url;
                if (!string.IsNullOrWhiteSpace(method))
                {
                    if (!baseUrl.EndsWith("/"))
                    {
                        baseUrl = $"{baseUrl}/";
                    }

                    if (method.StartsWith("/"))
                    {
                        method = method.Substring(1);
                    }

                    baseUrl = $"{baseUrl}{method}";
                }
            }
            else
            {
                baseUrl = method;
            }

            if (queryParams == null || !queryParams.Any())
            {
                return baseUrl;
            }

            var builder = new UriBuilder(baseUrl)
            {
                Port = -1,
            };

            var query = HttpUtility.ParseQueryString(builder.Query);
            queryParams.DoForEach(x => query.Add(x.Key, x.Value.ToString()));
            builder.Query = query.ToString();

            string url = builder.ToString();

            _logger.Info($"Built url for GET request: {url}");
            return url;
        }

        private async Task<T> ProcessResultAsync<T>(HttpResponseMessage response)
            where T : class
        {
            CheckResult(response);

            if (string.Equals(response.Content.Headers.ContentType.CharSet, "utf8", StringComparison.OrdinalIgnoreCase))
            {
                response.Content.Headers.ContentType.CharSet = "utf-8";
            }

            var result = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(result) || result == "[]")
            {
                return default(T);
            }

            try
            {
                if (typeof(T) == typeof(string))
                {
                    return result as T;
                }

                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (JsonSerializationException exp)
            {
                _logger.Error(result);
                _logger.Error(exp);

                throw new Exception($"Can't covert to {typeof(T)}. Response: {result}", exp);
            }
        }

        private static void FillHeaders(IDictionary<string, object> headers, HttpRequestMessage msg)
        {
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    msg.Headers.Add(header.Key, header.Value.ToString());
                }
            }
        }
    }
}