package com.allseating.android

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.allseating.android.data.Repository
import com.allseating.android.ui.edit.EditViewModel
import com.allseating.android.util.DateProvider

class EditViewModelFactory(
    private val repository: Repository,
    private val dateProvider: DateProvider,
    private val gameId: String?
) : ViewModelProvider.Factory {
    @Suppress("UNCHECKED_CAST")
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(EditViewModel::class.java)) {
            return EditViewModel(repository, dateProvider) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}
