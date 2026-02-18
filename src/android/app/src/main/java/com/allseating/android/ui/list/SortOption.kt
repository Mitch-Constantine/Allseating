package com.allseating.android.ui.list

data class SortOption(
    val label: String,
    val sortProp: String,
    val sortDir: String
) {
    fun matches(prop: String, dir: String): Boolean = sortProp == prop && sortDir == dir
}

object SortOptions {
    val ALL = listOf(
        SortOption("Title A\u2013Z", "title", "asc"),
        SortOption("Title Z\u2013A", "title", "desc"),
        SortOption("Price low\u2013high", "price", "asc"),
        SortOption("Price high\u2013low", "price", "desc"),
        SortOption("Release date newest", "releaseDate", "desc"),
        SortOption("Release date oldest", "releaseDate", "asc")
    )
}
