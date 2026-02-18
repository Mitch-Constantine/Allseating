package com.allseating.android

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.RadioGroup
import com.google.android.material.bottomsheet.BottomSheetDialogFragment
import com.allseating.android.ui.list.SortOptions

class FilterSortBottomSheetFragment : BottomSheetDialogFragment() {

    interface Callback {
        fun onApply(platform: String?, status: String?, sortProp: String, sortDir: String)
    }

    private var callback: Callback? = null

    fun setCallback(cb: Callback?) {
        callback = cb
    }

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, savedInstanceState: Bundle?): View? {
        return inflater.inflate(R.layout.dialog_filter_sort, container, false)
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)
        // Cap height so ScrollView can scroll when content is tall (e.g. on small screens)
        view.layoutParams = view.layoutParams?.apply {
            height = (resources.displayMetrics.heightPixels * 0.85f).toInt()
        }
        val platformGroup = view.findViewById<RadioGroup>(R.id.filter_platform)
        val statusGroup = view.findViewById<RadioGroup>(R.id.filter_status)
        val sortGroup = view.findViewById<RadioGroup>(R.id.filter_sort)

        val currentPlatform = arguments?.getString(ARG_PLATFORM)
        val currentStatus = arguments?.getString(ARG_STATUS)
        val currentSortProp = arguments?.getString(ARG_SORT_PROP) ?: "title"
        val currentSortDir = arguments?.getString(ARG_SORT_DIR) ?: "asc"

        selectPlatform(platformGroup, currentPlatform)
        selectStatus(statusGroup, currentStatus)
        selectSort(sortGroup, currentSortProp, currentSortDir)

        view.findViewById<View>(R.id.filter_clear).setOnClickListener {
            platformGroup.check(R.id.platform_all)
            statusGroup.check(R.id.status_all)
            sortGroup.check(R.id.sort_title_asc)
        }

        view.findViewById<View>(R.id.filter_apply).setOnClickListener {
            val platform = platformValue(platformGroup.checkedRadioButtonId)
            val status = statusValue(statusGroup.checkedRadioButtonId)
            val sortIndex = when (sortGroup.checkedRadioButtonId) {
                R.id.sort_title_asc -> 0
                R.id.sort_title_desc -> 1
                R.id.sort_price_asc -> 2
                R.id.sort_price_desc -> 3
                R.id.sort_release_desc -> 4
                R.id.sort_release_asc -> 5
                else -> 0
            }
            val sort = SortOptions.ALL[sortIndex]
            callback?.onApply(
                platform.takeIf { it.isNotEmpty() },
                status.takeIf { it.isNotEmpty() },
                sort.sortProp,
                sort.sortDir
            )
            dismiss()
        }
    }

    private fun selectPlatform(group: RadioGroup, value: String?) {
        val id = when (value) {
            null, "" -> R.id.platform_all
            "PC" -> R.id.platform_pc
            "PS5" -> R.id.platform_ps5
            "PS4" -> R.id.platform_ps4
            "XBOX_SERIES" -> R.id.platform_xbox_series
            "XBOX_ONE" -> R.id.platform_xbox_one
            "SWITCH" -> R.id.platform_switch
            else -> R.id.platform_all
        }
        group.check(id)
    }

    private fun platformValue(checkedId: Int): String = when (checkedId) {
        R.id.platform_pc -> "PC"
        R.id.platform_ps5 -> "PS5"
        R.id.platform_ps4 -> "PS4"
        R.id.platform_xbox_series -> "XBOX_SERIES"
        R.id.platform_xbox_one -> "XBOX_ONE"
        R.id.platform_switch -> "SWITCH"
        else -> ""
    }

    private fun selectStatus(group: RadioGroup, value: String?) {
        val id = when (value) {
            null, "" -> R.id.status_all
            "Upcoming" -> R.id.status_upcoming
            "Active" -> R.id.status_active
            "Discontinued" -> R.id.status_discontinued
            else -> R.id.status_all
        }
        group.check(id)
    }

    private fun statusValue(checkedId: Int): String = when (checkedId) {
        R.id.status_upcoming -> "Upcoming"
        R.id.status_active -> "Active"
        R.id.status_discontinued -> "Discontinued"
        else -> ""
    }

    private fun selectSort(group: RadioGroup, prop: String, dir: String) {
        val id = when {
            prop == "title" && dir == "asc" -> R.id.sort_title_asc
            prop == "title" && dir == "desc" -> R.id.sort_title_desc
            prop == "price" && dir == "asc" -> R.id.sort_price_asc
            prop == "price" && dir == "desc" -> R.id.sort_price_desc
            prop == "releaseDate" && dir == "desc" -> R.id.sort_release_desc
            prop == "releaseDate" && dir == "asc" -> R.id.sort_release_asc
            else -> R.id.sort_title_asc
        }
        group.check(id)
    }

    companion object {
        private const val ARG_PLATFORM = "platform"
        private const val ARG_STATUS = "status"
        private const val ARG_SORT_PROP = "sortProp"
        private const val ARG_SORT_DIR = "sortDir"

        fun newInstance(platform: String?, status: String?, sortProp: String, sortDir: String): FilterSortBottomSheetFragment {
            return FilterSortBottomSheetFragment().apply {
                arguments = Bundle().apply {
                    putString(ARG_PLATFORM, platform)
                    putString(ARG_STATUS, status)
                    putString(ARG_SORT_PROP, sortProp)
                    putString(ARG_SORT_DIR, sortDir)
                }
            }
        }
    }
}
