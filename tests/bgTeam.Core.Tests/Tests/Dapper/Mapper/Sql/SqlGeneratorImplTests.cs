﻿using bgTeam.Core.Tests.Infrastructure;
using bgTeam.DataAccess;
using bgTeam.DataAccess.Impl.Dapper;
using DapperExtensions;
using DapperExtensions.Mapper;
using DapperExtensions.Mapper.Sql;
using System;
using System.Collections.Generic;
using Xunit;

namespace bgTeam.Core.Tests.Dapper.Mapper.Sql
{
    public class SqlGeneratorImplTests
    {
        [Fact]
        public void SelectShouldThrowExceptionIfParametersIsNull()
        {
            var mapper = SqlHelper.GetMapper<TestingClass>();
            var sqlGenerator = GetSqlGenerator();
            Assert.Throws<ArgumentNullException>("parameters", () =>
            {
                sqlGenerator.Select(mapper, null, null, null);
            });
        }

        [Fact]
        public void Select() 
        {
            var mapper = SqlHelper.GetMapper<TestingClass>();
            var sqlGenerator = GetSqlGenerator();
            var query = sqlGenerator.Select(mapper, GetPredicateForId(), GetSortById(), new Dictionary<string, object>());
            Assert.Equal("SELECT id AS \"Id\" FROM \"TestingClass\" WHERE (id = @Id_0) ORDER BY id ASC", query);
        }

        [Fact]
        public void SelectPagedShouldThrowExceptionIfSortIsNullOrEmpty()
        {
            var mapper = SqlHelper.GetMapper<TestingClass>();
            var sqlGenerator = GetSqlGenerator();
            Assert.Throws<ArgumentNullException>("sort", () =>
            {
                sqlGenerator.SelectPaged(mapper, null, null, 1, 10, null);
            });
            Assert.Throws<ArgumentNullException>("sort", () =>
            {
                sqlGenerator.SelectPaged(mapper, null, new List<ISort>(), 1, 10, null);
            });
        }

        [Fact]
        public void SelectPagedShouldThrowExceptionIfParametersIsNull()
        {
            var mapper = SqlHelper.GetMapper<TestingClass>();
            var sqlGenerator = GetSqlGenerator();
            Assert.Throws<ArgumentNullException>("parameters", () =>
            {
                sqlGenerator.SelectPaged(mapper, null, GetSortById(), 1, 10, null);
            });
        }

        [Fact]
        public void SelectPaged()
        {
            var mapper = SqlHelper.GetMapper<TestingClass>();
            var sqlGenerator = GetSqlGenerator();
            var dictionary = new Dictionary<string, object>();
            var query = sqlGenerator.SelectPaged(mapper, GetPredicateForId(), GetSortById(), 2, 10, dictionary);
            Assert.Equal("SELECT id AS \"Id\" FROM \"TestingClass\" WHERE (id = @Id_0) ORDER BY id ASC LIMIT @Offset, @Count", query);
            Assert.Equal(1, dictionary["@Id_0"]);
            Assert.Equal(20, dictionary["@Offset"]);
            Assert.Equal(10, dictionary["@Count"]);
        }

        [Fact]
        public void SelectSetShouldThrowExceptionIfSortIsNullOrEmpty()
        {
            var mapper = SqlHelper.GetMapper<TestingClass>();
            var sqlGenerator = GetSqlGenerator();
            Assert.Throws<ArgumentNullException>("sort", () =>
            {
                sqlGenerator.SelectSet(mapper, null, null, 1, 10, null);
            });
            Assert.Throws<ArgumentNullException>("sort", () =>
            {
                sqlGenerator.SelectSet(mapper, null, new List<ISort>(), 1, 10, null);
            });
        }

        [Fact]
        public void SelectSetShouldThrowExceptionIfParametersIsNull()
        {
            var mapper = SqlHelper.GetMapper<TestingClass>();
            var sqlGenerator = GetSqlGenerator();
            Assert.Throws<ArgumentNullException>("parameters", () =>
            {
                sqlGenerator.SelectSet(mapper, null, GetSortById(), 1, 10, null);
            });
        }

        [Fact]
        public void SelectSet()
        {
            var mapper = SqlHelper.GetMapper<TestingClass>();
            var sqlGenerator = GetSqlGenerator();
            var dictionary = new Dictionary<string, object>();
            var query = sqlGenerator.SelectSet(mapper, GetPredicateForId(), GetSortById(), 2, 10, dictionary);
            Assert.Equal("SELECT id AS \"Id\" FROM \"TestingClass\" WHERE (id = @Id_0) ORDER BY id ASC LIMIT @Offset, @Count", query);
            Assert.Equal(1, dictionary["@Id_0"]);
            Assert.Equal(2, dictionary["@Offset"]);
            Assert.Equal(10, dictionary["@Count"]);
        }

        [Fact]
        public void CountShouldThrowExceptionIfParametersIsNull()
        {
            var sqlGenerator = GetSqlGenerator();
            Assert.Throws<ArgumentNullException>("parameters", () =>
            {
                sqlGenerator.Count(null, null, null);
            });
        }

        [Fact]
        public void Count()
        {
            var mapper = SqlHelper.GetMapper<TestingClass>();
            var sqlGenerator = GetSqlGenerator();
            var dictionary = new Dictionary<string, object>();
            var query = sqlGenerator.Count(mapper, GetPredicateForId(), dictionary);
            Assert.Equal("SELECT COUNT(*) AS \"Total\" FROM \"TestingClass\" WHERE (id = @Id_0)", query);
            Assert.Equal(1, dictionary["@Id_0"]);
        }

        [Fact]
        public void Insert()
        {
            var mapper = SqlHelper.GetMapper<TestingClass>();
            var sqlGenerator = GetSqlGenerator();
            var query = sqlGenerator.Insert(mapper);
            Assert.Equal("INSERT INTO \"TestingClass\" (id) VALUES (@Id)", query);
        }

        [Fact]
        public void InsertShouldThrowExceptionIfMappedColumnsCountIs0()
        {
            var sqlGenerator = GetSqlGenerator();
            Assert.Throws<ArgumentException>(() =>
            {
                sqlGenerator.Insert(new ClassMapper<TestingClass>());
            });
        }

        [Fact]
        public void UpdateShouldThrowExceptionIfPredicateIsNull()
        {
            var sqlGenerator = GetSqlGenerator();
            Assert.Throws<ArgumentNullException>("predicate", () =>
            {
                sqlGenerator.Update(SqlHelper.GetMapper<TestingClass>(), null, null);
            });
        }

        [Fact]
        public void UpdateShouldThrowExceptionIfParametersIsNull()
        {
            var sqlGenerator = GetSqlGenerator();
            Assert.Throws<ArgumentNullException>("parameters", () =>
            {
                sqlGenerator.Update(SqlHelper.GetMapper<TestingClass>(), GetPredicateForId(), null);
            });
        }

        [Fact]
        public void UpdateShouldThrowExceptionIfMappedColumnsCountIs0()
        {
            var sqlGenerator = GetSqlGenerator();
            Assert.Throws<ArgumentException>(() =>
            {
                sqlGenerator.Update(new ClassMapper<TestingClass>(), GetPredicateForId(), new Dictionary<string, object>());
            });
        }

        [Fact]
        public void Update()
        {
            var mapper = SqlHelper.GetMapper<TestingClass>();
            var sqlGenerator = GetSqlGenerator();
            var dictionary = new Dictionary<string, object>();
            var query = sqlGenerator.Update(mapper, GetGroupPredicate(), dictionary);
            Assert.Equal("UPDATE \"TestingClass\" SET id = @Id WHERE ((id = @Id_0) AND (id <= @Id_1))", query);
            Assert.Equal(1, dictionary["@Id_0"]);
        }

        [Fact]
        public void DeleteShouldThrowExceptionIfPredicateIsNull()
        {
            var sqlGenerator = GetSqlGenerator();
            Assert.Throws<ArgumentNullException>("predicate", () =>
            {
                sqlGenerator.Delete(SqlHelper.GetMapper<TestingClass>(), null, null);
            });
        }

        [Fact]
        public void DeleteShouldThrowExceptionIfParametersIsNull()
        {
            var sqlGenerator = GetSqlGenerator();
            Assert.Throws<ArgumentNullException>("parameters", () =>
            {
                sqlGenerator.Delete(SqlHelper.GetMapper<TestingClass>(), GetPredicateForId(), null);
            });
        }

        [Fact]
        public void Delete()
        {
            var mapper = SqlHelper.GetMapper<TestingClass>();
            var sqlGenerator = GetSqlGenerator();
            var dictionary = new Dictionary<string, object>();
            var query = sqlGenerator.Delete(mapper, GetPredicateForId(), dictionary);
            Assert.Equal("DELETE FROM \"TestingClass\" WHERE (id = @Id_0)", query);
            Assert.Equal(1, dictionary["@Id_0"]);
        }

        [Fact]
        public void GetColumnNameShouldThrowExceptionIfPropertMapIsNull()
        {
            var sqlGenerator = GetSqlGenerator();
            Assert.Throws<ArgumentException>(() =>
            {
                sqlGenerator.GetColumnName(SqlHelper.GetMapper<TestingClass>(), "Name", true);
            });
        }

        private FieldPredicate<TestingClass> GetPredicateForId()
        {
            return new FieldPredicate<TestingClass>()
            {
                PropertyName = "Id",
                Operator = Operator.Eq,
                Value = 1
            };
        }

        private PredicateGroup GetGroupPredicate()
        {
            return new PredicateGroup()
            {
                Operator = GroupOperator.And,
                Predicates = new List<IPredicate>()
                {
                    GetPredicateForId(),
                    new FieldPredicate<TestingClass>()
                    {
                        PropertyName = "Id",
                        Operator = Operator.Le,
                        Value = 15
                    }
                }
            };
        }

        private List<ISort> GetSortById()
        {
            return new List<ISort>()
            {
                Predicates.Sort<TestingClass>(x => x.Id)
            };
        }

        private ISqlGenerator GetSqlGenerator()
        {
            return SqlHelper.GetSqlGenerator();
        }

    }

    class TestingClass
    {
        [ColumnName("id")]
        public int Id { get; set; }
    }
}
