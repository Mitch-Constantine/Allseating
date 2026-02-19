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

# System Structure

## Backend (ASP.NET Core)

The backend follows a layered structure:

-   **Domain** -- Core entities and business rules
-   **Application** -- Use-case orchestration
-   **Infrastructure** -- EF Core implementation
-   **API** -- HTTP endpoints

Key implementation details:

-   EF Core Code-First
-   JSON seed data
-   Proper separation between layers
-   Concurrency handling
-   Uniqueness validation (e.g., barcode)
-   Unit and integration tests

Business logic is not embedded directly in controllers.

------------------------------------------------------------------------

## Android Application

The Android app consumes the backend API.

### Browse Screen

-   Displays games from the API
-   Supports sorting and filtering
-   Navigates to edit screen

### Edit Screen

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

# Why This Approach

I had limited prior Android experience.

To close that gap, I:

-   Studied modern Android architecture patterns
-   Implemented this project end-to-end
-   Used AI as a development accelerator while maintaining full
    understanding of the code

The result is a structured Android client integrated with a properly
layered backend.

------------------------------------------------------------------------

# What This Demonstrates

-   Ability to reinterpret requirements across platforms
-   Backend architecture discipline
-   Proper Android layering and state handling
-   Testing beyond minimal scaffolding
-   Rapid ramp-up in a new ecosystem

------------------------------------------------------------------------

# Running the Project

### Backend

-   Open solution
-   Apply migrations
-   Run API

### Android

-   Open in Android Studio
-   Configure base API URL
-   Run on emulator or device
