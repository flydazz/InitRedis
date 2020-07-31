using Dapper;
using InitRedis.Models;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace InitRedis
{
    class Program
    {

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");

            var configuration = builder.Build();

            var sqlConn = configuration.GetConnectionString("Connection");
            var redisConn = configuration.GetSection("Redis:Configuration").Value;

            IDatabase _db = null;

            try
            {
                var _connection = ConnectionMultiplexer.Connect(redisConn);
                _db = _connection.GetDatabase();
            }
            catch (Exception e)
            {
                throw new Exception("Redis 连接失败" + e);
            }

            using (IDbConnection conn = new SqlConnection(sqlConn))
            {
                conn.Open();

                List<PostModel> list = conn.Query<PostModel>("select * from posts").ToList();

                foreach (var item in list)
                {
                    SetRedisAccount(_db, item);
                }

            }

            Console.WriteLine("初始化成功！");
        }
        private static void SetRedisAccount(IDatabase _db, PostModel post)
        {
            string postId = $"Post:{post.Id}";
            string category = post.CategoryId?.ToString() ?? "";

            //redis中不存在，添加
            _db.ScriptEvaluate(
                UPDATE_POST,
                keys: new RedisKey[] { postId, "PostCategoryId" },
                values: new RedisValue[] { category }) ;

            _db.ScriptEvaluate(
                UPDATE_POST,
                keys: new RedisKey[] { postId, "PostTitle" },
                values: new RedisValue[] { post.Title });

            _db.ScriptEvaluate(
                UPDATE_POST,
                keys: new RedisKey[] { postId, "PostContent" },
                values: new RedisValue[] { post.Content });

        }

        const string UPDATE_POST = @"
local key = nil
local field = nil
local value = nil

if(KEYS[1])
then
    key=KEYS[1]
end
if(KEYS[2])
then
    field=KEYS[2]
end
if(ARGV[1])
then
    value=ARGV[1]
end

redis.call('hset', key, field, value)

";
    }
}
