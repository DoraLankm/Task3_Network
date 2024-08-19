using ConsoleApp21.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleApp21
{
    internal class MessageUDP
    {
        public Command Command { get; set; } // Тип команды
        public int? Id { get; set; } // Идентификатор сообщения
        public string FromName { get; set; } // Имя отправителя
        public string ToName { get; set; } // Имя получателя
        public string Text { get; set; } // Текст сообщения
        public DateTime Time { get; set; } //Время сообщения 


        public string ToJson()
        {
            try
            {
                return JsonSerializer.Serialize(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        public static MessageUDP FromJson(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<MessageUDP>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public override string ToString()
        {
            return $">> Сообщение от {FromName} ({Time.ToString()}):\n>> {Text}";
        }


    }
}
