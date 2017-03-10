using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MiniProject.Models
{
    public class MessageContacts
    {
        public MessageContacts()
        {
            MessageCollection = new List<MessageContacts>();
        }
        [Required(ErrorMessage = @"Message is required")]
        [DataType(DataType.MultilineText)]
        [RegularExpression(@"^[a-zA-Z0-9 ,.'@]+$", ErrorMessage = "Special character should not be entered")]
        public string MessageText { get; set; }
        public string Contact { get; set; }
        public DateTime SendDate { get; set; }
        public string SendTime { get; set; }
        public List<MessageContacts> MessageCollection {get; set;}
    }
}