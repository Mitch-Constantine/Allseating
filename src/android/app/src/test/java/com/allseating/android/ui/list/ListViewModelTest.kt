package com.allseating.android.ui.list

import androidx.arch.core.executor.testing.InstantTaskExecutorRule
import com.allseating.android.data.GameListItemDto
import com.allseating.android.data.GamesListResponse
import com.allseating.android.data.Repository
import com.allseating.android.ui.Result
import com.allseating.android.util.getOrAwaitValue
import io.mockk.coEvery
import io.mockk.coVerify
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
import kotlin.test.assertNull

@OptIn(ExperimentalCoroutinesApi::class)
@RunWith(RobolectricTestRunner::class)
@Config(sdk = [28])
class ListViewModelTest {

    @get:Rule
    val instantTask = InstantTaskExecutorRule()

    private val testDispatcher = UnconfinedTestDispatcher()
    private val repository = mockk<Repository>()
    private lateinit var viewModel: ListViewModel

    @Before
    fun setUp() {
        kotlinx.coroutines.Dispatchers.setMain(testDispatcher)
    }

    @After
    fun tearDown() {
        kotlinx.coroutines.Dispatchers.resetMain()
    }

    @Test
    fun load_callsRepositoryWithDefaultParams() = runTest(testDispatcher) {
        coEvery { repository.getGames(any(), any(), any(), any(), any(), any(), any()) } returns Result.Success(GamesListResponse(emptyList(), 0))
        viewModel = ListViewModel(repository)
        viewModel.load(append = false)
        coVerify(exactly = 1) { repository.getGames(0, 20, "title", "asc", null, null, null) }
    }

    @Test
    fun load_onSuccess_updatesUiStateWithItems() = runTest(testDispatcher) {
        val items = listOf(
            GameListItemDto("id1", "b1", "Title", "desc", "PC", "2025-01-01", "Active", 9.99)
        )
        coEvery { repository.getGames(any(), any(), any(), any(), any(), any(), any()) } returns Result.Success(GamesListResponse(items, 1))
        viewModel = ListViewModel(repository)
        viewModel.load(append = false)
        val state = viewModel.uiState.getOrAwaitValue()
        assertEquals(items, state.items)
        assertEquals(1, state.totalCount)
        assertEquals(false, state.loading)
        assertNull(state.error)
    }

    @Test
    fun load_onError_updatesUiStateWithError() = runTest(testDispatcher) {
        coEvery { repository.getGames(any(), any(), any(), any(), any(), any(), any()) } returns Result.Error("Network error")
        viewModel = ListViewModel(repository)
        viewModel.load(append = false)
        val state = viewModel.uiState.getOrAwaitValue()
        assertEquals("Network error", state.error)
        assertEquals(false, state.loading)
    }

    @Test
    fun retry_callsLoad() = runTest(testDispatcher) {
        coEvery { repository.getGames(any(), any(), any(), any(), any(), any(), any()) } returns Result.Success(GamesListResponse(emptyList(), 0))
        viewModel = ListViewModel(repository)
        viewModel.load(append = false)
        viewModel.retry()
        coVerify(exactly = 2) { repository.getGames(0, 20, "title", "asc", null, null, null) }
    }
}
