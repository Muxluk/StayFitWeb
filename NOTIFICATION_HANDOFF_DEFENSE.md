# Handoff: Система сповіщень у StayFit

## 1. Що реалізовано

В проєкті додано повноцінну систему сповіщень для користувача:

- додавання продукту в щоденник
- видалення продукту з щоденника
- встановлення денної цілі калорій
- перевищення денної норми калорій
- читання/очищення сповіщень у UI (дзвіночок)

Також виправлено критичний баг з різними UserId:

- раніше сповіщення створювались для DomainUsers.Id
- а читались по AspNetUsers claim Id
- через це сповіщення не відображались
- тепер NotificationController використовує DomainUserId через email

## 2. Основні зміни по шарах

### Domain

- нова сутність: `Notification`
- новий інтерфейс репозиторію: `INotificationRepository`

Файли:
- `StayFitWeb/src/StayFit/StayFit.Domain/Entities/Notification.cs`
- `StayFitWeb/src/StayFit/StayFit.Domain/Interfaces/INotificationRepository.cs`

### Infrastructure

- додано `DbSet<Notification>` в `AppDbContext`
- додано конфігурацію таблиці Notifications (ключ, індекси)
- додано реалізацію `NotificationRepository`
- додано DI реєстрацію `INotificationRepository`
- додано DI реєстрацію `INotificationService`
- додано EF migration для таблиці Notifications

Файли:
- `StayFitWeb/src/StayFit/StayFit.Infrastructure/Data/AppDBContext.cs`
- `StayFitWeb/src/StayFit/StayFit.Infrastructure/Repositories/NotificationRepository.cs`
- `StayFitWeb/src/StayFit/StayFit.Infrastructure/DependencyInjection.cs`
- `StayFitWeb/src/StayFit/StayFit.Infrastructure/Migrations/20260423131054_AddNotificationEntity.cs`

### Application

- додано `INotificationService`
- додано `NotificationService`
- додано `NotificationSettings` (поріг калорій)
- `FoodService` розширено: створення FoodAdded + перевірка порогу калорій
- `QuickAddService` розширено: створення сповіщень при quick add
- додано нормалізацію старих російських текстів у `NotificationService` при читанні

Файли:
- `StayFitWeb/src/StayFit/StayFit.Application/Interfaces/INotificationService.cs`
- `StayFitWeb/src/StayFit/StayFit.Application/Services/NotificationService.cs`
- `StayFitWeb/src/StayFit/StayFit.Application/Options/NotificationSettings.cs`
- `StayFitWeb/src/StayFit/StayFit.Application/Interfaces/FoodService.cs`
- `StayFitWeb/src/StayFit/StayFit.Application/Services/QuickAddService.cs`

### Web

- додано `NotificationController`:
  - `GET /notifications/unread`
  - `GET /notifications/unread-count`
  - `POST /notifications/{id}/mark-as-read`
  - `POST /notifications/mark-all-as-read`
  - `POST /notifications/clear-all`
- в `DiaryController` додано сповіщення на видалення
- в `NutritionGoalController` додано сповіщення на встановлення цілі
- в `FoodController` додано логування і проходження через сервіс створення сповіщень
- у layout додано UI дзвіночок + dropdown
- у `site.js` додано клієнтський менеджер сповіщень (polling 30 сек)

Файли:
- `StayFitWeb/src/StayFit/StayFit.Web/Controllers/NotificationController.cs`
- `StayFitWeb/src/StayFit/StayFit.Web/Controllers/DiaryController.cs`
- `StayFitWeb/src/StayFit/StayFit.Web/Controllers/NutritionGoalController.cs`
- `StayFitWeb/src/StayFit/StayFit.Web/Controllers/FoodController.cs`
- `StayFitWeb/src/StayFit/StayFit.Web/Views/Shared/_Layout.cshtml`
- `StayFitWeb/src/StayFit/StayFit.Web/wwwroot/js/site.js`

## 3. Конфігурація

Додано секцію:

```json
"Notifications": {
  "CalorieThresholdPercent": 100
}
```

Файли:
- `StayFitWeb/src/StayFit/StayFit.Web/appsettings.json`
- `StayFitWeb/src/StayFit/StayFit.Web/appsettings.Development.json`

`100` означає: сповіщення про калорії з'являється, коли досягнуто або перевищено 100% від цілі.

## 4. Що показувати на захисті (покроковий сценарій)

1. Запустити застосунок.
2. Увійти під користувачем.
3. Відкрити Nutrition Goal, зберегти ціль (наприклад 1800 ккал).
4. Відкрити Diary, додати продукт.
5. Показати, що в дзвіночку з'явилось сповіщення FoodAdded.
6. Видалити продукт з Diary.
7. Показати сповіщення FoodRemoved.
8. Додати їжу так, щоб перевищити норму.
9. Показати сповіщення про перевищення калорій.
10. У dropdown натиснути "Прочитати все" і "Очистити все".

## 5. Де міняти тексти сповіщень

Основний файл:
- `StayFitWeb/src/StayFit/StayFit.Application/Services/NotificationService.cs`

Методи з текстами:
- `CreateFoodAddedNotificationAsync`
- `CreateFoodRemovedNotificationAsync`
- `CreateNutritionGoalSetNotificationAsync`
- `CreateCalorieThresholdNotificationAsync`

## 6. Що питатимуть на захисті і як відповідати

### П: Чому спочатку не працювало?
В: Через різні ідентифікатори користувача в різних частинах системи (AspNetUserId vs DomainUserId). Створення і читання сповіщень йшли по різних UserId.

### П: Як виправили?
В: `NotificationController` переведено на отримання DomainUser через email, щоб UserId співпадав з тим, який використовується під час створення сповіщень у сервісах.

### П: Чому інколи старі повідомлення були російською?
В: Це старі записи в БД. Додано нормалізацію тексту при читанні в `NotificationService`, щоб старі записи теж відображались українською.

## 7. Перевірка/тести

Юніт тести для сервісу сповіщень:
- `StayFitWeb/src/StayFit/StayFit.Tests/Services/NotificationServiceTests.cs`

Команда:

```powershell
cd StayFitWeb/src/StayFit/StayFit.Tests
dotnet test --filter "FullyQualifiedName~NotificationServiceTests"
```

## 8. Технічні нотатки для того, хто захищає

- Якщо build падає з lock-помилками `.dll`: завершити всі `dotnet` процеси і запустити заново.
- Якщо сповіщення не оновилось одразу в UI: оновити сторінку (Ctrl+F5), бо polling йде раз на 30 секунд.
- "Dashboard N: 7" в UI бере значення з `Dashboard.RecentDiaryEntriesCount` в appsettings.

---

Якщо треба коротка версія на 2 хвилини виступу:

"Ми реалізували full-stack систему сповіщень: від сутності та міграцій до API і UI-дзвіночка. Покрили 4 бізнес-сценарії: додавання/видалення їжі, встановлення цілі, перевищення калорій. В процесі відловили і виправили баг несумісності UserId між Identity та Domain моделями. Додали логування та тести NotificationService, щоб система була стабільна і прозора для дебагу."