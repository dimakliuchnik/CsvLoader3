using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CsvLoader3.Models
{
    [MongoCollectionName("LoginPassword")]
    public class LoginModel
    {
        [Key]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        [BsonElement("Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [BsonElement("Password")]
        public string Password { get; set; }
    }
}