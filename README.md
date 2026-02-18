# Allseating Video Games Catalogue

## Design Choices

### Business Rules & Optimistic Concurrency

The demo is intentionally simple. It uses lightweight business rules,
including optimistic concurrency control, to show realistic backend
behavior and support solid unit testing.

This provides non-trivial scenarios beyond basic CRUD and reflects
patterns commonly used in production systems.

### Data Seeding

The database is automatically seeded with representative data on
startup. This allows proper testing of:

- Paging
- Sorting
- Search

The seed ensures the UI behaves correctly under realistic data volume.

### Testing Strategy

The project includes testing at multiple levels:

- **Backend unit tests** focused on business logic and concurrency scenarios
- **Android unit tests** for ViewModels and business logic
- **End-to-end tests** validating critical user flows

The project demonstrates testing across logic, component, and full
integration levels --- not only isolated mocks.

------------------------------------------------------------------------

## How to Run

From the `src` folder:

```powershell
.\start-android.ps1
```

This script will:

- Build and start the .NET API (backend)
- Optionally install and launch the Android app on a connected device or emulator (use `-ApiOnly` to skip the app)

Run without a device to have only the API; run with a device/emulator to use the Android app.

**Backend (Swagger):** http://localhost:5117/swagger (when using the default HTTP profile).

------------------------------------------------------------------------

## Alternative Manual Run

**Backend:**

- Open the API project in Visual Studio or run from `src/api/Allseating.Api`:
  - `dotnet run --launch-profile http` (or https if configured)

**Android app:**

- Open the `src/android` folder in Android Studio.
- Ensure the Allseating .NET API is running (e.g. http://localhost:5117).
- Run the app on an emulator or device.

------------------------------------------------------------------------

No additional setup is required. The database is created automatically
using SQL Server LocalDB.
