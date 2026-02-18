package com.allseating.android.ui.edit

import androidx.arch.core.executor.testing.InstantTaskExecutorRule
import com.allseating.android.data.GameDetailDto
import com.allseating.android.data.Repository
import com.allseating.android.ui.Result
import com.allseating.android.util.DateProvider
import com.allseating.android.util.getOrAwaitValue
import io.mockk.coEvery
import io.mockk.mockk
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.UnconfinedTestDispatcher
import kotlinx.coroutines.test.resetMain
import kotlinx.coroutines.test.runTest
import kotlinx.coroutines.test.setMain
import org.junit.After
import org.junit.Before
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config
import kotlin.test.assertEquals
import kotlin.test.assertNotNull
import kotlin.test.assertNull

/**
 * Matches Angular game-form.component.spec.ts: release date vs status validation.
 * DateProvider is fixed to "2025-06-15" (UTC today in tests).
 */
@OptIn(ExperimentalCoroutinesApi::class)
@RunWith(RobolectricTestRunner::class)
@Config(sdk = [28])
class EditViewModelTest {

    @get:Rule
    val instantTask = InstantTaskExecutorRule()

    private val utcToday = "2025-06-15"
    private val dateProvider = mockk<DateProvider>()
    private val repository = mockk<Repository>()
    private lateinit var viewModel: EditViewModel

    private fun validBarcode() = "BAR123"
    private fun validTitle() = "A Game"
    private fun validDescription() = "Description"
    private fun validPlatform() = "PC"
    private fun validPrice() = 19.99

    @Before
    fun setUp() {
        kotlinx.coroutines.Dispatchers.setMain(UnconfinedTestDispatcher())
        coEvery { dateProvider.getUtcToday() } returns utcToday
    }

    @After
    fun tearDown() {
        kotlinx.coroutines.Dispatchers.resetMain()
    }

    @Test
    fun save_Upcoming_withNullReleaseDate_setsSaveError() = runTest {
        viewModel = EditViewModel(repository, dateProvider)
        viewModel.save(
            id = null,
            barcode = validBarcode(),
            title = validTitle(),
            description = validDescription(),
            platform = validPlatform(),
            releaseDate = null,
            status = "Upcoming",
            price = validPrice()
        )
        val state = viewModel.uiState.getOrAwaitValue()
        assertNotNull(state.saveError)
        assertEquals(true, state.saveError!!.contains("Release date is required"))
    }

    @Test
    fun save_Active_withNullReleaseDate_setsSaveError() = runTest {
        viewModel = EditViewModel(repository, dateProvider)
        viewModel.save(
            id = null,
            barcode = validBarcode(),
            title = validTitle(),
            description = validDescription(),
            platform = validPlatform(),
            releaseDate = null,
            status = "Active",
            price = validPrice()
        )
        val state = viewModel.uiState.getOrAwaitValue()
        assertNotNull(state.saveError)
        assertEquals(true, state.saveError!!.contains("Release date is required"))
    }

    @Test
    fun save_Discontinued_withNullReleaseDate_noRequiredError() = runTest {
        coEvery { repository.createGame(any()) } returns Result.Success(
            GameDetailDto("id", "b", "t", "d", "PC", null, "Discontinued", 9.99, "rv")
        )
        viewModel = EditViewModel(repository, dateProvider)
        viewModel.save(
            id = null,
            barcode = validBarcode(),
            title = validTitle(),
            description = validDescription(),
            platform = validPlatform(),
            releaseDate = null,
            status = "Discontinued",
            price = validPrice()
        )
        val state = viewModel.uiState.getOrAwaitValue()
        assertNull(state.saveError)
    }

    @Test
    fun save_Upcoming_whenReleaseDateOnOrBeforeToday_setsStatusReleaseDateError() = runTest {
        viewModel = EditViewModel(repository, dateProvider)
        viewModel.save(
            id = null,
            barcode = validBarcode(),
            title = validTitle(),
            description = validDescription(),
            platform = validPlatform(),
            releaseDate = "2025-06-15",
            status = "Upcoming",
            price = validPrice()
        )
        var state = viewModel.uiState.getOrAwaitValue()
        assertNotNull(state.saveError)
        assertEquals(true, state.saveError!!.contains("after the current date"))
        viewModel.clearSaveError()
        viewModel.save(
            id = null,
            barcode = validBarcode(),
            title = validTitle(),
            description = validDescription(),
            platform = validPlatform(),
            releaseDate = "2025-06-14",
            status = "Upcoming",
            price = validPrice()
        )
        state = viewModel.uiState.getOrAwaitValue()
        assertNotNull(state.saveError)
        assertEquals(true, state.saveError!!.contains("after the current date"))
    }

    @Test
    fun save_Upcoming_whenReleaseDateAfterToday_validates() = runTest {
        coEvery { repository.createGame(any()) } returns Result.Success(
            GameDetailDto("id", "b", "t", "d", "PC", "2025-06-16", "Upcoming", 9.99, "rv")
        )
        viewModel = EditViewModel(repository, dateProvider)
        viewModel.save(
            id = null,
            barcode = validBarcode(),
            title = validTitle(),
            description = validDescription(),
            platform = validPlatform(),
            releaseDate = "2025-06-16",
            status = "Upcoming",
            price = validPrice()
        )
        val state = viewModel.uiState.getOrAwaitValue()
        assertNull(state.saveError)
    }

    @Test
    fun save_Active_whenReleaseDateAfterToday_setsStatusReleaseDateError() = runTest {
        viewModel = EditViewModel(repository, dateProvider)
        viewModel.save(
            id = null,
            barcode = validBarcode(),
            title = validTitle(),
            description = validDescription(),
            platform = validPlatform(),
            releaseDate = "2025-06-16",
            status = "Active",
            price = validPrice()
        )
        val state = viewModel.uiState.getOrAwaitValue()
        assertNotNull(state.saveError)
        assertEquals(true, state.saveError!!.contains("on or before the current date"))
    }

    @Test
    fun save_Active_whenReleaseDateOnOrBeforeToday_validates() = runTest {
        coEvery { repository.createGame(any()) } returns Result.Success(
            GameDetailDto("id", "b", "t", "d", "PC", "2025-06-15", "Active", 9.99, "rv")
        )
        viewModel = EditViewModel(repository, dateProvider)
        viewModel.save(
            id = null,
            barcode = validBarcode(),
            title = validTitle(),
            description = validDescription(),
            platform = validPlatform(),
            releaseDate = "2025-06-15",
            status = "Active",
            price = validPrice()
        )
        var state = viewModel.uiState.getOrAwaitValue()
        assertNull(state.saveError)
        viewModel.clearSaveError()
        viewModel.save(
            id = null,
            barcode = validBarcode(),
            title = validTitle(),
            description = validDescription(),
            platform = validPlatform(),
            releaseDate = "2025-06-14",
            status = "Active",
            price = validPrice()
        )
        state = viewModel.uiState.getOrAwaitValue()
        assertNull(state.saveError)
    }

    @Test
    fun save_Discontinued_whenReleaseDateInFuture_setsStatusReleaseDateError() = runTest {
        viewModel = EditViewModel(repository, dateProvider)
        viewModel.save(
            id = null,
            barcode = validBarcode(),
            title = validTitle(),
            description = validDescription(),
            platform = validPlatform(),
            releaseDate = "2025-06-16",
            status = "Discontinued",
            price = validPrice()
        )
        val state = viewModel.uiState.getOrAwaitValue()
        assertNotNull(state.saveError)
        assertEquals(true, state.saveError!!.contains("must not be in the future"))
    }

    @Test
    fun save_Discontinued_whenReleaseDateNullOrPast_validates() = runTest {
        coEvery { repository.createGame(any()) } returns Result.Success(
            GameDetailDto("id", "b", "t", "d", "PC", null, "Discontinued", 9.99, "rv")
        )
        viewModel = EditViewModel(repository, dateProvider)
        viewModel.save(
            id = null,
            barcode = validBarcode(),
            title = validTitle(),
            description = validDescription(),
            platform = validPlatform(),
            releaseDate = null,
            status = "Discontinued",
            price = validPrice()
        )
        var state = viewModel.uiState.getOrAwaitValue()
        assertNull(state.saveError)
        viewModel.clearSaveError()
        coEvery { repository.createGame(any()) } returns Result.Success(
            GameDetailDto("id", "b", "t", "d", "PC", "2025-06-14", "Discontinued", 9.99, "rv")
        )
        viewModel.save(
            id = null,
            barcode = validBarcode(),
            title = validTitle(),
            description = validDescription(),
            platform = validPlatform(),
            releaseDate = "2025-06-14",
            status = "Discontinued",
            price = validPrice()
        )
        state = viewModel.uiState.getOrAwaitValue()
        assertNull(state.saveError)
    }
}
