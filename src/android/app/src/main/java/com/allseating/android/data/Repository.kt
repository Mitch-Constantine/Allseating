package com.allseating.android.data

import com.allseating.android.ui.Result
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import org.json.JSONObject
import retrofit2.HttpException
import java.io.IOException

class Repository(private val api: ApiService) {

    /**
     * Parses API error body (e.g. ProblemDetails). Returns (detailMessage, errorCode).
     * Angular uses body.detail for message and body.errorCode only for 409 ConcurrencyConflict on update.
     */
    private fun parseErrorBody(e: HttpException): Pair<String?, String?> {
        val body = e.response()?.errorBody()?.string() ?: return Pair(null, null)
        return try {
            val json = JSONObject(body)
            val detail = json.optString("detail").takeIf { it.isNotEmpty() }
            val code = when {
                json.has("errorCode") -> json.optString("errorCode").takeIf { it.isNotEmpty() }
                json.has("extensions") -> json.optJSONObject("extensions")?.optString("errorCode")?.takeIf { it.isNotEmpty() }
                else -> null
            }
            Pair(detail, code)
        } catch (_: Exception) {
            Pair(null, null)
        }
    }

    suspend fun getGames(
        offset: Int = 0,
        limit: Int = 50,
        sortProp: String? = "title",
        sortDir: String? = "asc",
        q: String? = null,
        platform: String? = null,
        status: String? = null
    ): Result<GamesListResponse> = withContext(Dispatchers.IO) {
        try {
            val response = api.getList(
                offset = offset,
                limit = limit,
                sortProp = sortProp,
                sortDir = sortDir,
                q = q,
                platform = platform,
                status = status
            )
            Result.Success(response)
        } catch (e: HttpException) {
            Result.Error(messageFromHttp(e))
        } catch (e: IOException) {
            Result.Error(e.message ?: "Network error")
        }
    }

    suspend fun getGameById(id: String): Result<GameDetailDto> = withContext(Dispatchers.IO) {
        try {
            val response = api.getById(id)
            Result.Success(response)
        } catch (e: HttpException) {
            if (e.code() == 404) Result.Error("Game not found")
            else Result.Error(messageFromHttp(e))
        } catch (e: IOException) {
            Result.Error(e.message ?: "Network error")
        }
    }

    suspend fun createGame(dto: CreateGameDto): Result<GameDetailDto> = withContext(Dispatchers.IO) {
        try {
            val response = api.create(dto)
            Result.Success(response)
        } catch (e: HttpException) {
            Result.Error(messageFromHttp(e))
        } catch (e: IOException) {
            Result.Error(e.message ?: "Network error")
        }
    }

    suspend fun updateGame(id: String, dto: UpdateGameDto): Result<GameDetailDto> = withContext(Dispatchers.IO) {
        try {
            val response = api.update(id, dto)
            Result.Success(response)
        } catch (e: HttpException) {
            if (e.code() == 409) {
                val (detail, errorCode) = parseErrorBody(e)
                if (errorCode == "ConcurrencyConflict") {
                    Result.ConcurrencyConflict(detail ?: "Conflict: game was modified. Reload and try again.")
                } else {
                    Result.Error(detail ?: "Conflict (409).")
                }
            } else {
                Result.Error(messageFromHttp(e))
            }
        } catch (e: IOException) {
            Result.Error(e.message ?: "Network error")
        }
    }

    suspend fun deleteGame(id: String): Result<Unit> = withContext(Dispatchers.IO) {
        try {
            api.delete(id)
            Result.Success(Unit)
        } catch (e: HttpException) {
            Result.Error(messageFromHttp(e))
        } catch (e: IOException) {
            Result.Error(e.message ?: "Network error")
        }
    }

    private fun messageFromHttp(e: HttpException): String {
        val (detail, _) = parseErrorBody(e)
        return detail ?: "Error ${e.code()}: ${e.message()}"
    }
}
