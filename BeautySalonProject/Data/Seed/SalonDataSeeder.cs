using BeautySalonProject.Models;
using Microsoft.EntityFrameworkCore;

namespace BeautySalonProject.Data.Seed
{
    public static class SalonDataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            // ------------------ CATEGORIES ------------------
            if (!await db.ServiceCategories.AnyAsync())
            {
                var categories = new List<ServiceCategory>
                {
                    new ServiceCategory { Name = "Фризьор", IsActive = true},
                    new ServiceCategory { Name = "Маникюр", IsActive = true },
                    new ServiceCategory { Name = "Педикюр", IsActive = true },
                    new ServiceCategory { Name = "Козметика", IsActive = true },
                    new ServiceCategory { Name = "Епилация", IsActive = true },
                    new ServiceCategory { Name = "Вежди и мигли", IsActive = true },
                    new ServiceCategory { Name = "Грим", IsActive = true }
                };

                await db.ServiceCategories.AddRangeAsync(categories);
                await db.SaveChangesAsync();
            }

            var categoriesFromDb = await db.ServiceCategories.ToListAsync();

            var hair = categoriesFromDb.First(c => c.Name == "Фризьор");
            var nails = categoriesFromDb.First(c => c.Name == "Маникюр");
            var pedicure = categoriesFromDb.First(c => c.Name == "Педикюр");
            var cosmetics = categoriesFromDb.First(c => c.Name == "Козметика");
            var laser = categoriesFromDb.First(c => c.Name == "Епилация");
            var brows = categoriesFromDb.First(c => c.Name == "Вежди и мигли");
            var makeup = categoriesFromDb.First(c => c.Name == "Грим");

            // ------------------ EMPLOYEES ------------------
            if (!await db.Employees.AnyAsync())
            {
                var employees = new List<Employee>
                {
                    new Employee { FirstName = "Мария", LastName = "Иванова", Phone = "08912457688", Email = "mariag678@gmail.com", IsActive = true, JobTitle = "Фризьор", PrimaryCategoryId = hair.CategoryId },
                    new Employee {FirstName = "Елена", LastName = "Петрова", Phone = "0872340970", Email = "elenai908@gmail.com", IsActive = true,JobTitle = "Маникюрист", PrimaryCategoryId = nails.CategoryId},
                    new Employee { FirstName = "Десислава", LastName = "Георгиева",  Phone = "0887604040", Email = "desis376@gmail.com",IsActive = true, JobTitle = "Педикюрист", PrimaryCategoryId = pedicure.CategoryId },
                    new Employee {FirstName = "Гергана", LastName = "Стоянова", Phone = "0897783355", Email = "gerganar769@gmail.com",IsActive = true, JobTitle = "Козметик", PrimaryCategoryId = cosmetics.CategoryId},
                    new Employee {FirstName = "Ива", LastName = "Николова", Phone = "0895611306", Email = "ivaivi914@gmail.com", IsActive = true,JobTitle = "Лазерен специалист", PrimaryCategoryId = laser.CategoryId},
                    new Employee {FirstName = "Ралица", LastName = "Костова", Phone = "0884407891", Email = "ralitsa765@gmail.com", IsActive = true,JobTitle = "Брау артист", PrimaryCategoryId = brows.CategoryId},
                    new Employee {FirstName = "Силвия", LastName = "Тодорова", Phone = "0873279950", Email = "silviasisi689@gmail.com", IsActive = true,JobTitle = "Гримьор", PrimaryCategoryId = makeup.CategoryId}
                };

                await db.Employees.AddRangeAsync(employees);
                await db.SaveChangesAsync();
            }

            var employeesFromDb = await db.Employees.ToListAsync();

            var e1 = employeesFromDb.First(e => e.FirstName == "Мария");
            var e2 = employeesFromDb.First(e => e.FirstName == "Елена");
            var e3 = employeesFromDb.First(e => e.FirstName == "Десислава");
            var e4 = employeesFromDb.First(e => e.FirstName == "Гергана");
            var e5 = employeesFromDb.First(e => e.FirstName == "Ива");
            var e6 = employeesFromDb.First(e => e.FirstName == "Ралица");
            var e7 = employeesFromDb.First(e => e.FirstName == "Силвия");

            if (!await db.Services.AnyAsync())
            {
                var services = new List<Service>
    {
        new Service { Name = "Подстригване", CategoryId = hair.CategoryId, EmployeeId = e1.EmployeeId, IsActive = true },
        new Service { Name = "Боядисване", CategoryId = hair.CategoryId, EmployeeId = e1.EmployeeId, IsActive = true },
        new Service { Name = "Прическа", CategoryId = hair.CategoryId, EmployeeId = e1.EmployeeId, IsActive = true },
        new Service { Name = "Терапии", CategoryId = hair.CategoryId, EmployeeId = e1.EmployeeId, IsActive = true },
        new Service { Name = "Измиване + сешоар", CategoryId = hair.CategoryId, EmployeeId = e1.EmployeeId, IsActive = true },

        new Service { Name = "Маникюр", CategoryId = nails.CategoryId, EmployeeId = e2.EmployeeId, IsActive = true },
        new Service { Name = "Сваляне/Декорации", CategoryId = nails.CategoryId, EmployeeId = e2.EmployeeId, IsActive = true },
        new Service { Name = "Поддръжка", CategoryId = nails.CategoryId, EmployeeId = e2.EmployeeId, IsActive = true },
        new Service { Name = "Ноктопластика", CategoryId = nails.CategoryId, EmployeeId = e2.EmployeeId, IsActive = true },

        new Service { Name = "Педикюр", CategoryId = pedicure.CategoryId, EmployeeId = e3.EmployeeId, IsActive = true },

        new Service { Name = "Почистване на лице", CategoryId = cosmetics.CategoryId, EmployeeId = e4.EmployeeId, IsActive = true },
        new Service { Name = "Пилинг", CategoryId = cosmetics.CategoryId, EmployeeId = e4.EmployeeId, IsActive = true },
        new Service { Name = "Масаж на лице", CategoryId = cosmetics.CategoryId, EmployeeId = e4.EmployeeId, IsActive = true },
        new Service { Name = "Процедури", CategoryId = cosmetics.CategoryId, EmployeeId = e4.EmployeeId, IsActive = true },

        new Service { Name = "Лазерна епилация", CategoryId = laser.CategoryId, EmployeeId = e5.EmployeeId, IsActive = true },

        new Service { Name = "Оформяне на вежди", CategoryId = brows.CategoryId, EmployeeId = e6.EmployeeId, IsActive = true },
        new Service { Name = "Ламиниране", CategoryId = brows.CategoryId, EmployeeId = e6.EmployeeId, IsActive = true },
        new Service { Name = "Микроблейдинг", CategoryId = brows.CategoryId, EmployeeId = e6.EmployeeId, IsActive = true },
        new Service { Name = "Миглопластика", CategoryId = brows.CategoryId, EmployeeId = e6.EmployeeId, IsActive = true },

        new Service { Name = "Дневен грим", CategoryId = makeup.CategoryId, EmployeeId = e7.EmployeeId, IsActive = true },
        new Service { Name = "Официален грим", CategoryId = makeup.CategoryId, EmployeeId = e7.EmployeeId, IsActive = true }
    };

                await db.Services.AddRangeAsync(services);
                await db.SaveChangesAsync();
            }
            var s = await db.Services
       .ToDictionaryAsync(x => x.Name, x => x);
            if (!await db.ServiceVariants.AnyAsync())
            {

                var variants = new List<ServiceVariant>
{
   // Подстригване
   new ServiceVariant { VariantName = "Мъжко подстригване", Price = 10, DurationMinutes = 30, IsActive = true, ServiceId = s["Подстригване"].ServiceId },
   new ServiceVariant { VariantName = "Дамско подстригване", Price = 15, DurationMinutes = 45, IsActive = true, ServiceId = s["Подстригване"].ServiceId },
   new ServiceVariant { VariantName = "Детско подстригване", Price = 10, DurationMinutes = 30, IsActive = true, ServiceId = s["Подстригване"].ServiceId },

   // Боядисване
   new ServiceVariant { VariantName = "Корени", Price = 23, DurationMinutes = 90, IsActive = true, ServiceId = s["Боядисване"].ServiceId },
   new ServiceVariant { VariantName = "Цяла коса - къса", Price = 30, DurationMinutes = 120,IsActive = true, ServiceId = s["Боядисване"].ServiceId },
   new ServiceVariant { VariantName = "Цяла коса - дълга", Price = 40, DurationMinutes = 150, IsActive = true, ServiceId = s["Боядисване"].ServiceId },
   new ServiceVariant { VariantName = "Кичури", Price = 5, DurationMinutes = 180, IsActive = true, ServiceId = s["Боядисване"].ServiceId },
   new ServiceVariant { VariantName = "Балеаж/Омбре", Price = 5, DurationMinutes = 210, IsActive = true, ServiceId = s["Боядисване"].ServiceId },

   // Прическа
   new ServiceVariant { VariantName = "Ежедневна - къса коса", Price = 25, DurationMinutes = 40, IsActive = true, ServiceId = s["Прическа"].ServiceId },
   new ServiceVariant { VariantName = "Ежедневна - дълга коса", Price = 40, DurationMinutes = 60, IsActive = true, ServiceId = s["Прическа"].ServiceId },
   new ServiceVariant { VariantName = "Официална - къса коса", Price = 45, DurationMinutes = 75, IsActive = true, ServiceId = s["Прическа"].ServiceId },
   new ServiceVariant { VariantName = "Официална - дълга коса", Price = 60, DurationMinutes = 90,IsActive = true, ServiceId = s["Прическа"].ServiceId },

   // Терапии
   new ServiceVariant { VariantName = "Възстановяваща терапия - къса коса", Price = 25, DurationMinutes = 45, IsActive = true, ServiceId = s["Терапии"].ServiceId },
   new ServiceVariant { VariantName = "Възстановяваща терапия - дълга коса", Price = 35, DurationMinutes = 60, IsActive = true, ServiceId = s["Терапии"].ServiceId },
   new ServiceVariant { VariantName = "Кератин - къса коса", Price = 35, DurationMinutes = 120, IsActive = true, ServiceId = s["Терапии"].ServiceId },
   new ServiceVariant { VariantName = "Кератин - дълга коса", Price = 45, DurationMinutes = 180, IsActive = true, ServiceId = s["Терапии"].ServiceId },

   // Сешоар
   new ServiceVariant { VariantName = "Къса коса", Price = 8, DurationMinutes = 30, IsActive = true, ServiceId = s["Измиване + сешоар"].ServiceId },
   new ServiceVariant { VariantName = "Средна коса", Price = 10, DurationMinutes = 40, IsActive = true, ServiceId = s["Измиване + сешоар"].ServiceId },
   new ServiceVariant { VariantName = "Дълга коса", Price = 13, DurationMinutes = 50, IsActive = true, ServiceId = s["Измиване + сешоар"].ServiceId },

   // Маникюр
   new ServiceVariant { VariantName = "Класически маникюр", Price = 10, DurationMinutes = 40, IsActive = true, ServiceId = s["Маникюр"].ServiceId },
   new ServiceVariant { VariantName = "Гел лак - къси нокти", Price = 18, DurationMinutes = 70, IsActive = true, ServiceId = s["Маникюр"].ServiceId },
   new ServiceVariant { VariantName = "Гел лак - дълги нокти", Price = 23, DurationMinutes = 90, IsActive = true, ServiceId = s["Маникюр"].ServiceId },
   new ServiceVariant { VariantName = "Изграждане - къси нокти", Price = 30, DurationMinutes = 120, IsActive = true, ServiceId = s["Маникюр"].ServiceId },
   new ServiceVariant { VariantName = "Изграждане - дълги нокти", Price = 40, DurationMinutes = 150, IsActive = true, ServiceId = s["Маникюр"].ServiceId },

   // Сваляне
   new ServiceVariant { VariantName = "Сваляне на гел лак", Price = 5, DurationMinutes = 20, IsActive = true, ServiceId = s["Сваляне/Декорации"].ServiceId },
   new ServiceVariant { VariantName = "Декорация (1-2 нокътя)", Price = 3, DurationMinutes = 10, IsActive = true, ServiceId = s["Сваляне/Декорации"].ServiceId },
   new ServiceVariant { VariantName = "Декорация (пълен сет)", Price = 8, DurationMinutes = 20, IsActive = true, ServiceId = s["Сваляне/Декорации"].ServiceId },

   // Поддръжка
   new ServiceVariant { VariantName = "Поддръжка - къси нокти", Price = 20, DurationMinutes = 105, IsActive = true, ServiceId = s["Поддръжка"].ServiceId },
   new ServiceVariant { VariantName = "Поддръжка - дълги нокти", Price = 25, DurationMinutes = 120, IsActive = true, ServiceId = s["Поддръжка"].ServiceId },

   // Ноктопластика
   new ServiceVariant { VariantName = "Ноктопластика - къси нокти", Price = 35, DurationMinutes = 135, IsActive = true, ServiceId = s["Ноктопластика"].ServiceId },
   new ServiceVariant { VariantName = "Ноктопластика - дълги нокти", Price = 45, DurationMinutes = 165, IsActive = true, ServiceId = s["Ноктопластика"].ServiceId },

   // Педикюр
   new ServiceVariant { VariantName = "Класически педикюр", Price = 20, DurationMinutes = 60, IsActive = true, ServiceId = s["Педикюр"].ServiceId },
   new ServiceVariant { VariantName = "Медицински педикюр", Price = 27, DurationMinutes = 90, IsActive = true, ServiceId = s["Педикюр"].ServiceId },

   // Козметика
   new ServiceVariant { VariantName = "Класическо почистване", Price = 25, DurationMinutes = 60, IsActive = true, ServiceId = s["Почистване на лице"].ServiceId },
   new ServiceVariant { VariantName = "Ултразвуково почистване", Price = 33, DurationMinutes = 75, IsActive = true, ServiceId = s["Почистване на лице"].ServiceId },
   new ServiceVariant { VariantName = "Хидра почистване", Price = 40, DurationMinutes = 90, IsActive = true, ServiceId = s["Почистване на лице"].ServiceId },

   new ServiceVariant { VariantName = "Повърхностен пилинг", Price = 30, DurationMinutes = 45, IsActive = true, ServiceId = s["Пилинг"].ServiceId },
   new ServiceVariant { VariantName = "Среден пилинг", Price = 40, DurationMinutes = 60, IsActive = true, ServiceId = s["Пилинг"].ServiceId },

   new ServiceVariant { VariantName = "Релаксиращ масаж", Price = 18, DurationMinutes = 30, IsActive = true, ServiceId = s["Масаж на лице"].ServiceId },
   new ServiceVariant { VariantName = "Лифтинг масаж", Price = 23, DurationMinutes = 40, IsActive = true, ServiceId = s["Масаж на лице"].ServiceId },

   new ServiceVariant { VariantName = "Хидратираща процедура", Price = 35, DurationMinutes = 60, IsActive = true, ServiceId = s["Процедури"].ServiceId },
   new ServiceVariant { VariantName = "Анти-ейдж процедура", Price = 45, DurationMinutes = 75, IsActive = true, ServiceId = s["Процедури"].ServiceId },

   // Лазер
   new ServiceVariant { VariantName = "Лице", Price = 20, DurationMinutes = 20, IsActive = true, ServiceId = s["Лазерна епилация"].ServiceId },
   new ServiceVariant { VariantName = "Подмишници", Price = 22, DurationMinutes = 20, IsActive = true, ServiceId = s["Лазерна епилация"].ServiceId },
   new ServiceVariant { VariantName = "Ръце", Price = 30, DurationMinutes = 30, IsActive = true, ServiceId = s["Лазерна епилация"].ServiceId },
   new ServiceVariant { VariantName = "Крака", Price = 45, DurationMinutes = 40, IsActive = true, ServiceId = s["Лазерна епилация"].ServiceId },
   new ServiceVariant { VariantName = "Интим", Price = 25, DurationMinutes = 30, IsActive = true, ServiceId = s["Лазерна епилация"].ServiceId },
   new ServiceVariant { VariantName = "Цяло тяло", Price = 100, DurationMinutes = 90, IsActive = true, ServiceId = s["Лазерна епилация"].ServiceId },

   // Вежди
   new ServiceVariant { VariantName = "Оформяне", Price = 8, DurationMinutes = 20, IsActive = true, ServiceId = s["Оформяне на вежди"].ServiceId },
   new ServiceVariant { VariantName = "Оформяне + боядисване", Price = 13, DurationMinutes = 30, IsActive = true, ServiceId = s["Оформяне на вежди"].ServiceId },

   new ServiceVariant { VariantName = "Ламиниране на вежди", Price = 28, DurationMinutes = 60, IsActive = true, ServiceId = s["Ламиниране"].ServiceId },
   new ServiceVariant { VariantName = "Ламиниране на мигли", Price = 30, DurationMinutes = 60, IsActive = true, ServiceId = s["Ламиниране"].ServiceId },

   new ServiceVariant { VariantName = "Микроблейдинг - първа процедура", Price = 80, DurationMinutes = 120, IsActive = true, ServiceId = s["Микроблейдинг"].ServiceId },
   new ServiceVariant { VariantName = "Микроблейдинг - корекция", Price = 40, DurationMinutes = 60, IsActive = true, ServiceId = s["Микроблейдинг"].ServiceId },

   new ServiceVariant { VariantName = "Косъм по косъм", Price = 45, DurationMinutes = 120, IsActive = true, ServiceId = s["Миглопластика"].ServiceId },
   new ServiceVariant { VariantName = "Обем (2D-3D)", Price = 50, DurationMinutes = 150, IsActive = true, ServiceId = s["Миглопластика"].ServiceId },

   // Грим
   new ServiceVariant { VariantName = "Дневен грим", Price = 23, DurationMinutes = 60, IsActive = true, ServiceId = s["Дневен грим"].ServiceId },
   new ServiceVariant { VariantName = "Официален грим", Price = 40, DurationMinutes = 90, IsActive = true, ServiceId = s["Официален грим"].ServiceId }
};

                await db.ServiceVariants.AddRangeAsync(variants);
                await db.SaveChangesAsync();
            } 
        }
    }
}

