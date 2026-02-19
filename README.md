# Video Game Catalogue -- Android Client + .NET Backend

## Overview

This project implements a simple two-screen video game catalogue:

1.  Browse a list of games
2.  Edit an existing game entry

The emphasis is on clean structure, separation of concerns, and correct
use of framework features --- not UI polish.

This was originally inspired by a web-based exercise. Instead of
recreating it in a web stack, I rebuilt it as:

-   A structured ASP.NET Core backend
-   A native Android client consuming that API

The goal was to demonstrate cross-platform interpretation of
requirements and the ability to quickly ramp up in Android.

------------------------------------------------------------------------

## System Structure

### Backend (ASP.NET Core)

The backend follows a layered structure:

-   **Domain** -- Core entities and business rules
-   **Application** -- Use-case orchestration
-   **Infrastructure** -- EF Core implementation
-   **API** -- HTTP endpoints

Key implementation details:

-   EF Core Code-First
-   Local file-based database
-   Automatic database creation on startup
-   Migrations applied automatically at runtime
-   JSON seed data
-   Concurrency handling
-   Uniqueness validation (e.g., barcode)
-   Unit and integration tests

No manual migration steps are required. Running the API creates and
migrates the local database automatically.

Business logic is not embedded directly in controllers.

------------------------------------------------------------------------

### Android Application

The Android app consumes the backend API.

#### Browse Screen

-   Displays games from the API
-   Supports sorting and filtering
-   Navigates to edit screen

#### Edit Screen

-   Loads a selected game
-   Allows modification
-   Sends updates to backend
-   Handles validation and API errors

Architectural characteristics:

-   ViewModel layer for state management
-   Repository abstraction for API access
-   Dedicated API service layer
-   No business logic inside UI components
-   Unit tests for ViewModels
-   UI tests (Espresso)

------------------------------------------------------------------------

## Running the Project

### Backend

Simply run the API project.

-   The local database file will be created automatically.
-   Migrations are applied automatically on startup.
-   Seed data is inserted automatically.

No manual EF migration commands are required.

### Android

A helper script is included:

`start-android.ps1`

This script:

-   Starts the backend
-   Builds and launches the Android application

Note:

-   The script does not create an Android emulator.
-   An emulator or physical device must already be available.

Alternatively, the Android project can be opened directly in Android
Studio and run normally.

------------------------------------------------------------------------

## What This Demonstrates

-   Ability to reinterpret requirements across platforms
-   Backend architecture discipline
-   Proper Android layering and state handling
-   Testing beyond minimal scaffolding
-   Rapid ramp-up in a new ecosystem
