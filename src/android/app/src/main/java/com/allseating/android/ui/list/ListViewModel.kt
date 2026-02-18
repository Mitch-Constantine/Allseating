package com.allseating.android.ui.list

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.allseating.android.data.GameListItemDto
import com.allseating.android.data.Repository
import com.allseating.android.ui.Result
import kotlinx.coroutines.launch

data class ListUiState(
    val items: List<GameListItemDto> = emptyList(),
    val totalCount: Int = 0,
    val loading: Boolean = false,
    val loadingMore: Boolean = false,
    val error: String? = null
)

class ListViewModel(private val repository: Repository) : ViewModel() {

    private val _uiState = MutableLiveData<ListUiState>(ListUiState())
    val uiState: LiveData<ListUiState> = _uiState

    private var query: String? = null
    private var platform: String? = null
    private var status: String? = null
    private var sortProp: String = "title"
    private var sortDir: String = "asc"
    private var offset: Int = 0

    companion object {
        const val PAGE_SIZE = 20
    }

    fun setQuery(q: String?) {
        query = q
    }

    fun setFilters(platform: String?, status: String?) {
        this.platform = platform
        this.status = status
    }

    fun setSort(prop: String, dir: String) {
        sortProp = prop
        sortDir = dir
    }

    fun getCurrentPlatform(): String? = platform
    fun getCurrentStatus(): String? = status
    fun getSortProp(): String = sortProp
    fun getSortDir(): String = sortDir

    fun load(append: Boolean) {
        viewModelScope.launch {
            val current = _uiState.value ?: ListUiState()
            val currentItems = if (append) current.items else emptyList()
            val requestOffset = if (append) offset else 0

            if (append) {
                _uiState.value = current.copy(loadingMore = true, error = null)
            } else {
                _uiState.value = current.copy(loading = true, error = null)
            }

            when (val result = repository.getGames(
                offset = requestOffset,
                limit = PAGE_SIZE,
                sortProp = sortProp,
                sortDir = sortDir,
                q = query.takeIf { !it.isNullOrBlank() },
                platform = platform.takeIf { !it.isNullOrBlank() },
                status = status.takeIf { !it.isNullOrBlank() }
            )) {
                is Result.Success -> {
                    val newItems = if (append) currentItems + result.data.items else result.data.items
                    offset = newItems.size
                    _uiState.value = ListUiState(
                        items = newItems,
                        totalCount = result.data.totalCount,
                        loading = false,
                        loadingMore = false,
                        error = null
                    )
                }
                is Result.Error -> {
                    _uiState.value = current.copy(
                        loading = false,
                        loadingMore = false,
                        error = result.message
                    )
                }
                is Result.ConcurrencyConflict -> { /* getGames does not return this */ }
            }
        }
    }

    fun loadNextPage() {
        val state = _uiState.value ?: return
        if (state.loadingMore || state.loading) return
        if (state.items.size >= state.totalCount) return
        load(append = true)
    }

    fun retry() = load(append = false)
}
