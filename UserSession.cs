using System;

namespace KeyboardTrainer
{
    public static class UserSession
    {
        // Текущий пользователь
        public static int UserId { get; set; }
        public static string Login { get; set; }
        public static string Role { get; set; }
        public static bool IsBlocked { get; set; }

        // Метод для установки данных пользователя после авторизации
        public static void SetUser(int userId, string login, string role, bool isBlocked)
        {
            UserId = userId;
            Login = login;
            Role = role;
            IsBlocked = isBlocked;
        }

        // Метод для очистки сессии (при выходе)
        public static void Clear()
        {
            UserId = 0;
            Login = string.Empty;
            Role = string.Empty;
            IsBlocked = false;
        }

        // Проверка, авторизован ли пользователь
        public static bool IsLoggedIn()
        {
            return UserId > 0;
        }

        // Проверка, является ли пользователь администратором
        public static bool IsAdmin()
        {
            return IsLoggedIn() && Role?.ToLower() == "admin";
        }

        // Проверка, заблокирован ли пользователь
        public static bool IsUserBlocked()
        {
            return IsBlocked;
        }
    }
}