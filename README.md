# Maximum - Система авторизации

Проект ASP.NET Core с системой авторизации, использующий ASP.NET Core Identity и PostgreSQL.

## Функциональность

- ✅ Регистрация пользователей
- ✅ Вход в систему
- ✅ Выход из системы
- ✅ Просмотр профиля
- ✅ Управление сессиями
- ✅ Защищенные маршруты

## Технологии

- ASP.NET Core 8.0
- Entity Framework Core
- ASP.NET Core Identity
- PostgreSQL
- Bootstrap 5
- Font Awesome

## Настройка

### 1. Установка PostgreSQL

Убедитесь, что у вас установлен PostgreSQL. Создайте базу данных:

```sql
CREATE DATABASE MaximumDB;
```

### 2. Настройка строки подключения

Отредактируйте файл `Maximum/appsettings.json` и укажите ваши параметры подключения:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=MaximumDB;Username=postgres;Password=your_password;Port=5432"
  }
}
```

### 3. Создание миграций

Выполните команды для создания и применения миграций:

```bash
cd Maximum
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Запуск проекта

```bash
dotnet run
```

## Структура проекта

```
Maximum/
├── Controllers/
│   ├── AccountController.cs    # Контроллер авторизации
│   └── HomeController.cs       # Главный контроллер
├── Models/
│   ├── ApplicationUser.cs      # Модель пользователя
│   ├── LoginViewModel.cs       # Модель входа
│   └── RegisterViewModel.cs    # Модель регистрации
├── Views/
│   ├── Account/                # Представления авторизации
│   │   ├── Login.cshtml
│   │   ├── Register.cshtml
│   │   ├── Profile.cshtml
│   │   └── AccessDenied.cshtml
│   ├── Home/
│   │   └── Index.cshtml       # Главная страница
│   └── Shared/
│       └── _Layout.cshtml     # Общий макет
├── Data/
│   └── ApplicationDbContext.cs # Контекст базы данных
└── Program.cs                  # Конфигурация приложения
```

## Маршруты

- `/` - Главная страница
- `/Account/Login` - Вход в систему
- `/Account/Register` - Регистрация
- `/Account/Profile` - Профиль пользователя (требует авторизации)
- `/Account/Logout` - Выход из системы

## Особенности

- Адаптивный дизайн с Bootstrap 5
- Валидация форм на стороне клиента и сервера
- Безопасные cookies для аутентификации
- Поддержка "Запомнить меня"
- Красивый UI с иконками Font Awesome

## Требования

- .NET 8.0 SDK
- PostgreSQL 12+
- Современный веб-браузер

## Лицензия

MIT License
