package com.allseating.android

import androidx.test.core.app.ActivityScenario
import androidx.test.espresso.Espresso.onView
import androidx.test.espresso.assertion.ViewAssertions.matches
import androidx.test.espresso.matcher.ViewMatchers.isDisplayed
import androidx.test.espresso.matcher.ViewMatchers.withId
import androidx.test.ext.junit.runners.AndroidJUnit4
import androidx.test.filters.LargeTest
import org.junit.Test
import org.junit.runner.RunWith

/**
 * E2E-style test matching web e2e "Search drives grid".
 * Verifies list screen loads: RecyclerView or error/retry is visible.
 * (Android app does not have search box on list; we only assert list screen is shown.)
 */
@RunWith(AndroidJUnit4::class)
@LargeTest
class SearchListTest {

    @Test
    fun listScreen_showsRecyclerViewOrError() {
        ActivityScenario.launch(MainActivity::class.java)
        onView(withId(R.id.list_recycler)).check(matches(isDisplayed()))
    }
}
