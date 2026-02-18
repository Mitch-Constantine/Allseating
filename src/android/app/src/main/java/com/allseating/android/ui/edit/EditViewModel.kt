package com.allseating.android.ui.edit

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.allseating.android.data.CreateGameDto
import com.allseating.android.data.GameConstants
import com.allseating.android.data.GameDetailDto
import com.allseating.android.data.Repository
import com.allseating.android.data.UpdateGameDto
import com.allseating.android.ui.Result
import com.allseating.android.util.DateProvider
import kotlinx.coroutines.launch
import java.text.SimpleDateFormat
import java.util.Locale

data class EditUiState(
    val game: GameDetailDto? = null,
    val loading: Boolean = false,
    val loadError: String? = null,
    val saveError: String? = null,
    val saveSuccess: Boolean = false,
    val deleteSuccess: Boolean = false,
    val showConcurrencyDialog: Boolean = false
)

class EditViewModel(
    private val repository: Repository,
    private val dateProvider: DateProvider
) : ViewModel() {

    private val _uiState = MutableLiveData<EditUiState>(EditUiState())
    val uiState: LiveData<EditUiState> = _uiState

    private var currentRowVersion: String? = null

    fun loadGame(id: String) {
        viewModelScope.launch {
            _uiState.value = _uiState.value?.copy(loading = true, loadError = null) ?: EditUiState(loading = true)
            when (val result = repository.getGameById(id)) {
                is Result.Success -> {
                    currentRowVersion = result.data.rowVersion
                    _uiState.value = EditUiState(
                        game = result.data,
                        loading = false,
                        loadError = null
                    )
                }
                is Result.Error -> {
                    _uiState.value = _uiState.value?.copy(loading = false, loadError = result.message)
                        ?: EditUiState(loading = false, loadError = result.message)
                }
                is Result.ConcurrencyConflict -> { /* getGameById does not return this */ }
            }
        }
    }

    fun save(
        id: String?,
        barcode: String,
        title: String,
        description: String,
        platform: String,
        releaseDate: String?,
        status: String,
        price: Double
    ) {
        val barcodeTrim = barcode.trim()
        val titleTrim = title.trim()
        val descriptionTrim = description.trim()
        val releaseDateNorm = normalizeReleaseDate(releaseDate)

        val validationError = validate(barcodeTrim, titleTrim, descriptionTrim, platform, releaseDateNorm, status, price)
        if (validationError != null) {
            _uiState.value = _uiState.value?.copy(saveError = validationError)
            return
        }

        viewModelScope.launch {
            _uiState.value = _uiState.value?.copy(loading = true, saveError = null, saveSuccess = false)
            if (id != null) {
                val rv = currentRowVersion
                if (rv.isNullOrBlank()) {
                    _uiState.value = _uiState.value?.copy(loading = false, saveError = "RowVersion missing. Reload the game.")
                    return@launch
                }
                val dto = UpdateGameDto(
                    barcode = barcodeTrim,
                    title = titleTrim,
                    description = descriptionTrim,
                    platform = platform,
                    releaseDate = releaseDateNorm,
                    status = status,
                    price = price,
                    rowVersion = rv
                )
                when (val result = repository.updateGame(id, dto)) {
                    is Result.Success -> {
                        currentRowVersion = result.data.rowVersion
                        _uiState.value = EditUiState(
                            game = result.data,
                            loading = false,
                            saveError = null,
                            saveSuccess = true
                        )
                    }
                    is Result.Error -> {
                        _uiState.value = _uiState.value?.copy(loading = false, saveError = result.message)
                    }
                    is Result.ConcurrencyConflict -> {
                        _uiState.value = _uiState.value?.copy(
                            loading = false,
                            saveError = null,
                            showConcurrencyDialog = true
                        )
                    }
                }
            } else {
                val dto = CreateGameDto(
                    barcode = barcodeTrim,
                    title = titleTrim,
                    description = descriptionTrim,
                    platform = platform,
                    releaseDate = releaseDateNorm,
                    status = status,
                    price = price
                )
                when (val result = repository.createGame(dto)) {
                    is Result.Success -> {
                        _uiState.value = EditUiState(
                            game = result.data,
                            loading = false,
                            saveError = null,
                            saveSuccess = true
                        )
                    }
                    is Result.Error -> {
                        _uiState.value = _uiState.value?.copy(loading = false, saveError = result.message)
                    }
                    is Result.ConcurrencyConflict -> { /* create does not return this */ }
                }
            }
        }
    }

    private fun validate(
        barcode: String,
        title: String,
        description: String,
        platform: String,
        releaseDate: String?,
        status: String,
        price: Double
    ): String? {
        if (barcode.isBlank()) return "Barcode is required."
        if (barcode.length > 64) return "Barcode must not exceed 64 characters."
        if (title.isBlank()) return "Title is required."
        if (title.length > 200) return "Title must not exceed 200 characters."
        if (description.isBlank()) return "Description is required."
        if (description.length > 2000) return "Description must not exceed 2000 characters."
        if (price <= 0) return "Price must be greater than 0."
        if (platform !in GameConstants.PLATFORMS) return "Invalid platform."
        if (status !in GameConstants.STATUSES) return "Invalid status."
        val today = dateProvider.getUtcToday()
        val dateStr = releaseDate?.take(10)
        when (status) {
            "Upcoming", "Active" -> {
                if (dateStr.isNullOrBlank()) return "Release date is required when Status is Upcoming or Active."
            }
        }
        when (status) {
            "Upcoming" -> if (!dateStr.isNullOrBlank() && dateStr <= today) return "When Status is Upcoming, ReleaseDate must be after the current date."
            "Active" -> if (!dateStr.isNullOrBlank() && dateStr > today) return "When Status is Active, ReleaseDate must be on or before the current date."
            "Discontinued" -> if (!dateStr.isNullOrBlank() && dateStr > today) return "When Status is Discontinued, ReleaseDate must not be in the future."
        }
        return null
    }

    /** Parses common date input and returns API format yyyy-MM-dd to avoid 400 from backend. */
    private fun normalizeReleaseDate(s: String?): String? {
        if (s.isNullOrBlank()) return null
        val trimmed = s.trim().take(20)
        if (trimmed.isBlank()) return null
        return try {
            val fmtIn = SimpleDateFormat("yyyy-M-d", Locale.US)
            fmtIn.isLenient = true
            val date = fmtIn.parse(trimmed) ?: return trimmed
            SimpleDateFormat("yyyy-MM-dd", Locale.US).format(date)
        } catch (_: Exception) {
            trimmed.take(10)
        }
    }

    fun clearSaveError() {
        _uiState.value = _uiState.value?.copy(saveError = null)
    }

    fun clearSaveSuccess() {
        _uiState.value = _uiState.value?.copy(saveSuccess = false)
    }

    fun delete(id: String) {
        viewModelScope.launch {
            _uiState.value = _uiState.value?.copy(loading = true, saveError = null, deleteSuccess = false)
            when (val result = repository.deleteGame(id)) {
                is Result.Success -> {
                    _uiState.value = _uiState.value?.copy(loading = false, deleteSuccess = true)
                }
                is Result.Error -> {
                    _uiState.value = _uiState.value?.copy(loading = false, saveError = result.message)
                }
                is Result.ConcurrencyConflict -> { /* delete does not return this */ }
            }
        }
    }

    fun clearDeleteSuccess() {
        _uiState.value = _uiState.value?.copy(deleteSuccess = false)
    }

    fun reloadLatest(id: String) {
        viewModelScope.launch {
            _uiState.value = _uiState.value?.copy(loading = true, loadError = null, saveError = null, showConcurrencyDialog = false)
            when (val result = repository.getGameById(id)) {
                is Result.Success -> {
                    currentRowVersion = result.data.rowVersion
                    _uiState.value = EditUiState(
                        game = result.data,
                        loading = false,
                        loadError = null,
                        saveError = null
                    )
                }
                is Result.Error -> {
                    _uiState.value = _uiState.value?.copy(loading = false, loadError = result.message)
                }
                is Result.ConcurrencyConflict -> { /* getGameById does not return this */ }
            }
        }
    }

    fun clearConcurrencyDialog() {
        _uiState.value = _uiState.value?.copy(showConcurrencyDialog = false)
    }
}
