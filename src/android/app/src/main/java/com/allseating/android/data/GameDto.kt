package com.allseating.android.data

import com.google.gson.annotations.SerializedName

/**
 * DTOs matching the backend API (Allseating.Api.Dto).
 * Assumption: Id and ReleaseDate are serialized as strings; RowVersion as base64 string.
 */
data class GameListItemDto(
    @SerializedName("id") val id: String,
    @SerializedName("barcode") val barcode: String,
    @SerializedName("title") val title: String,
    @SerializedName("description") val description: String,
    @SerializedName("platform") val platform: String,
    @SerializedName("releaseDate") val releaseDate: String?,
    @SerializedName("status") val status: String,
    @SerializedName("price") val price: Double
)

data class GamesListResponse(
    @SerializedName("items") val items: List<GameListItemDto>,
    @SerializedName("totalCount") val totalCount: Int
)

data class GameDetailDto(
    @SerializedName("id") val id: String,
    @SerializedName("barcode") val barcode: String,
    @SerializedName("title") val title: String,
    @SerializedName("description") val description: String,
    @SerializedName("platform") val platform: String,
    @SerializedName("releaseDate") val releaseDate: String?,
    @SerializedName("status") val status: String,
    @SerializedName("price") val price: Double,
    @SerializedName("rowVersion") val rowVersion: String
)

data class CreateGameDto(
    @SerializedName("barcode") val barcode: String,
    @SerializedName("title") val title: String,
    @SerializedName("description") val description: String,
    @SerializedName("platform") val platform: String,
    @SerializedName("releaseDate") val releaseDate: String?,
    @SerializedName("status") val status: String,
    @SerializedName("price") val price: Double
)

data class UpdateGameDto(
    @SerializedName("barcode") val barcode: String,
    @SerializedName("title") val title: String,
    @SerializedName("description") val description: String,
    @SerializedName("platform") val platform: String,
    @SerializedName("releaseDate") val releaseDate: String?,
    @SerializedName("status") val status: String,
    @SerializedName("price") val price: Double,
    @SerializedName("rowVersion") val rowVersion: String
)

object GameConstants {
    val PLATFORMS = listOf("PC", "PS5", "PS4", "XBOX_SERIES", "XBOX_ONE", "SWITCH")
    val STATUSES = listOf("Upcoming", "Active", "Discontinued")
}
