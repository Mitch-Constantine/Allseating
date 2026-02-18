# Allseating Android App

Native Android (Kotlin) games list and edit UI for the Allseating API.

## Structure

- **app/src/main/java/com/allseating/android/**
  - **MainActivity** – list screen (RecyclerView, search, filter, sort, paging, progress, error, retry, New Game).
  - **EditActivity** – create/edit screen (load by id, form fields, save, delete).
  - **FilterSortBottomSheetFragment** – filter/sort bottom sheet (platform, status, sort).
  - **AllseatingApp** – Application; provides Retrofit **ApiService** and **Repository**.
  - **ListViewModelFactory** / **EditViewModelFactory** – create ViewModels with Repository.
  - **data/** – ApiService, GameDto.kt, Repository.
  - **ui/list/** – ListViewModel, ListUiState, GameAdapter, SortOption.
  - **ui/edit/** – EditViewModel, EditUiState.
  - **ui/Result.kt** – sealed class Success/Error/ConcurrencyConflict for repo results.

## Base URL

- Set in **BuildConfig.API_BASE_URL** (see `app/build.gradle.kts`).
- Default: `http://10.0.2.2:5117/` (emulator → host machine, .NET API HTTP port).
- Change the `buildConfigField` for your environment.

## Run

1. Open the `src/android` folder in Android Studio.
2. Ensure the Allseating .NET API is running (e.g. `http://localhost:5117`).
3. Run the app on an emulator or device (emulator uses `10.0.2.2` for host).

## Gradle wrapper

If `gradlew` / `gradlew.bat` are missing, generate them from the `src/android` directory:

```bash
gradle wrapper --gradle-version 8.2
```

Or let Android Studio create the wrapper when you open the project.

## Tests

- **Unit (JVM):** `./gradlew testDebugUnitTest`
  - **ListViewModelTest** – list API params, success/error state, retry.
  - **EditViewModelTest** – release date vs status validation (Upcoming/Active/Discontinued).
- **Instrumented (device/emulator):** `./gradlew connectedDebugAndroidTest`
  - **SearchListTest** – list screen shows RecyclerView.
  - **CreateThenListTest** – New Game → fill form → Save → back to list.
