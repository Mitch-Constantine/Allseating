package com.allseating.android

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.allseating.android.data.Repository
import com.allseating.android.ui.list.ListViewModel

class ListViewModelFactory(private val repository: Repository) : ViewModelProvider.Factory {
    @Suppress("UNCHECKED_CAST")
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(ListViewModel::class.java)) {
            return ListViewModel(repository) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}
