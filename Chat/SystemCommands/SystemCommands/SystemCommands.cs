using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemCommands
{
    public static class Symbols
    {
        public const char SEPARATOR = (char)0;
        public const char MSG_END = (char)1;
        public const char EXIT = (char)2;
        public const char ALL_CHAT = (char)3;
        public const char PRIVATE_CHAT = (char)4;
        public const char SELF_MSG = (char)5;
        public const char USER_LIST = (char)6;
        public const char NICKNAME_TAKEN = (char)7;

        /* КЛИЕНТ -> СЕРВЕР
         * <получатель>SEPARATOR<сообщение>SEPARATOR<отправитель>MSG_END - обычное сообщение
         * <получатель> = ALL_CHAT - общее сообщение
         * EXIT - отключение от сервера
         */

        /* СЕРВЕР -> КЛИЕНТ
         * <сообщение>SEPARATOR<время>SEPARATOR<отправитель>MSG_END - обычное сообщение
         * PRIVATE_CHAT<обычное сообщение> - личное сообщение
         * SELF_MSG<обычное сообщение> - свое личное сообщение
         * USER_LIST<имя пользователя>SEPARATOR<имя пользователя>...MSG_END - список пользователей
         */
    }
}
