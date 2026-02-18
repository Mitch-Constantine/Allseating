package com.allseating.android.data

import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Path
import retrofit2.http.Query

interface ApiService {

    @GET("api/games")
    suspend fun getList(
        @Query("offset") offset: Int = 0,
        @Query("limit") limit: Int = 50,
        @Query("sortProp") sortProp: String? = null,
        @Query("sortDir") sortDir: String? = null,
        @Query("q") q: String? = null,
        @Query("platform") platform: String? = null,
        @Query("status") status: String? = null
    ): GamesListResponse

    @GET("api/games/{id}")
    suspend fun getById(@Path("id") id: String): GameDetailDto

    @POST("api/games")
    suspend fun create(@Body dto: CreateGameDto): GameDetailDto

    @PUT("api/games/{id}")
    suspend fun update(@Path("id") id: String, @Body dto: UpdateGameDto): GameDetailDto

    @DELETE("api/games/{id}")
    suspend fun delete(@Path("id") id: String): Unit
}
