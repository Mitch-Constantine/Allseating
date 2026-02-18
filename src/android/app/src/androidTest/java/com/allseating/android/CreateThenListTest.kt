package com.allseating.android

import androidx.test.core.app.ActivityScenario
import androidx.test.espresso.Espresso.onView
import androidx.test.espresso.action.ViewActions.click
import androidx.test.espresso.action.ViewActions.typeText
import androidx.test.espresso.assertion.ViewAssertions.matches
import androidx.test.espresso.matcher.ViewMatchers.isDisplayed
import androidx.test.espresso.matcher.ViewMatchers.withId
import androidx.test.espresso.matcher.ViewMatchers.withText
import androidx.test.ext.junit.runners.AndroidJUnit4
import androidx.test.filters.LargeTest
import org.junit.Test
import org.junit.runner.RunWith

/**
 * E2E-style test matching web e2e "Create then search".
 * From list, open New Game, fill form, save; verify we return to list screen.
 */
@RunWith(AndroidJUnit4::class)
@LargeTest
class CreateThenListTest {

    @Test
    fun createGame_thenSave_returnsToListScreen() {
        val scenario = ActivityScenario.launch(MainActivity::class.java)
        scenario.onActivity { it.openOptionsMenu() }
        onView(withText("New Game")).perform(click())

        onView(withId(R.id.edit_barcode)).perform(typeText("E2E-BAR-" + System.currentTimeMillis()))
        onView(withId(R.id.edit_title)).perform(typeText("E2E-CREATE-" + System.currentTimeMillis()))
        onView(withId(R.id.edit_description)).perform(typeText("E2E test game"))
        onView(withId(R.id.edit_release_date)).perform(typeText("2027-01-15"))
        onView(withId(R.id.edit_price)).perform(typeText("29.99"))

        onView(withId(R.id.edit_save)).perform(click())

        onView(withId(R.id.list_recycler)).check(matches(isDisplayed()))
    }
}
