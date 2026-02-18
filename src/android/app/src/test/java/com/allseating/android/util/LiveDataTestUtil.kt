package com.allseating.android.util

import androidx.lifecycle.LiveData
import androidx.lifecycle.Observer
import java.util.concurrent.CountDownLatch
import java.util.concurrent.TimeUnit
import java.util.concurrent.atomic.AtomicReference

fun <T> LiveData<T>.getOrAwaitValue(timeout: Long = 2, unit: TimeUnit = TimeUnit.SECONDS): T {
    val ref = AtomicReference<T>()
    val latch = CountDownLatch(1)
    val observer = Observer<T> { t ->
        ref.set(t)
        latch.countDown()
    }
    observeForever(observer)
    latch.await(timeout, unit)
    removeObserver(observer)
    return ref.get()!!
}
