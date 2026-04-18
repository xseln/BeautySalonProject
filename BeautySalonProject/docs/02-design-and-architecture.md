1. Тип и структура на приложението

Тип на приложението

Проектът представлява уеб приложение за управление на салон за красота (Beauty Salon Management System).

Приложението позволява:

разглеждане на услуги;
записване на часове;
изпращане на запитвания;
управление от администратор.
Архитектура и организация

Проектът е реализиран с ASP.NET Core MVC:

Основни папки и модули:
-Controllers – основна логика (Home, Services, Inquiries)
-Areas
--Admin – администраторски панел
--Client – клиентска част
--Staff – служители
-Models – ентитита (Appointment, Service и др.)
-ViewModels – модели за изгледи
-Data – DbContext и конфигурации
-wwwroot – статични файлове (CSS, JS, изображения)
-Services – помощни услуги (EmailService)


2. Модел на данните

Основни ентитита:
👤 ApplicationUser
-FirstName
-LastName
-Email
-Role

📅 Appointment
-AppointmentId
-VariantId
-EmployeeId
-ClientUserId
-StartAt / EndAt
-Status
-GuestEmail
-FinalPrice

📌 Централна таблица в системата

💇 ServiceCategory
-CategoryId
-Name
-IsActive
-Service
-ServiceId
-CategoryId
-EmployeeId
-Name
-Description

💎 ServiceVariant
-VariantId
-ServiceId
-VariantName
-Price
-DurationMinutes

🧑‍🔧 Employee
-EmployeeId
-FirstName / LastName
-Phone / Email
-JobTitle

📆 EmployeeWorkDay
-EmployeeId
-Date
-StartTime
-EndTime

📩 Inquiry
-InquiryId
-FullName
-Email
-Phone
-Message
-Status

🔗 Връзки между таблиците
-ServiceCategory → Service (1:N)
-Service → ServiceVariant (1:N)
-ServiceVariant → Appointment (1:N)
-Employee → Appointment (1:N)
-Employee → EmployeeWorkDay (1:N)
-Inquiry → Appointment (1:N)
-ApplicationUser → Appointment (1:N)


3. Диаграма и структурирано описание



Основна идея:

Appointments е централната таблица
услугите са организирани в йерархия:
Category → Service → Variant
поддържат се както регистрирани потребители, така и гости


4. Потребителски поток

👤 Гост (нерегистриран):
-Влиза в сайта
-Преглежда услуги
-Изпраща запитване
-Получава имейл при одобрение/отказ

👤 Клиент (регистриран)
-Регистрация / вход
Избор на:
-категория
-услуга
-вариант
Избор на:
-служител
-дата и час
-Потвърждение на резервация
-Преглед на „Моите часове“

👨‍💼 Администратор
-Влиза в Admin панел
-Преглежда запитвания
-Потвърждава или отказва
-Системата изпраща email
-Управлява услуги и служители


5. Използвани технологии

Основни технологии:
-C# – основен програмен език
-ASP.NET Core MVC – framework за уеб приложение
-Entity Framework Core – ORM за работа с база данни
-SQL Server – релационна база данни

Frontend:
-HTML / CSS
-JavaScript
-Bootstrap

Допълнителни технологии:
-SMTP (Gmail) – за изпращане на имейли
-Identity – за автентикация и роли

Причини за избор:
-ASP.NET Core → модерен и мощен framework
-EF Core → улеснява работата с база данни
-SQL Server → стабилна и широко използвана
-Bootstrap → бърз и responsive дизайн
-Gmail SMTP → лесна интеграция за имейли