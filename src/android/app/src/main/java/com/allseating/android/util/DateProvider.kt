package com.allseating.android.util

import java.util.Calendar
import java.util.Locale
import java.util.TimeZone

/**
 * Provides current date in UTC YYYY-MM-DD for validation (matches Angular DateProviderService).
 */
interface DateProvider {
    fun getUtcToday(): String
}

class RealDateProvider : DateProvider {
    override fun getUtcToday(): String {
        val c = Calendar.getInstance(TimeZone.getTimeZone("UTC"), Locale.ROOT)
        return "%04d-%02d-%02d".format(c.get(Calendar.YEAR), c.get(Calendar.MONTH) + 1, c.get(Calendar.DAY_OF_MONTH))
    }
}
