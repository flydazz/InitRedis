using System;
using System.Collections.Generic;
using System.Text;

namespace InitRedis.Models
{
    public class PostModel
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 分类Id
        /// </summary>
        public int? CategoryId { get; set; }

    }
}
