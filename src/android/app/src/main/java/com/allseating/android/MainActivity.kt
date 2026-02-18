package com.allseating.android

import android.content.Intent
import android.os.Bundle
import android.os.Handler
import android.os.Looper
import android.view.Menu
import android.view.MenuItem
import android.view.View
import android.widget.ProgressBar
import android.widget.TextView
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import androidx.appcompat.widget.SearchView
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.allseating.android.data.GameListItemDto
import com.allseating.android.ui.list.GameAdapter
import com.allseating.android.ui.list.ListUiState
import com.allseating.android.ui.list.ListViewModel

class MainActivity : AppCompatActivity() {

    private val viewModel: ListViewModel by viewModels {
        ListViewModelFactory((application as AllseatingApp).repository)
    }

    private lateinit var recyclerView: RecyclerView
    private lateinit var progressBar: ProgressBar
    private lateinit var errorText: TextView
    private lateinit var retryButton: View
    private lateinit var loadingMoreFooter: View
    private lateinit var adapter: GameAdapter

    private val searchHandler = Handler(Looper.getMainLooper())
    private var searchRunnable: Runnable? = null
    private val searchDebounceMs = 400L

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)
        setSupportActionBar(findViewById(R.id.list_toolbar))

        recyclerView = findViewById(R.id.list_recycler)
        progressBar = findViewById(R.id.list_progress)
        errorText = findViewById(R.id.list_error)
        retryButton = findViewById(R.id.list_retry)
        loadingMoreFooter = findViewById(R.id.list_loading_more)

        adapter = GameAdapter { item -> openEdit(item.id) }
        val layoutManager = LinearLayoutManager(this)
        recyclerView.layoutManager = layoutManager
        recyclerView.adapter = adapter

        retryButton.setOnClickListener { viewModel.retry() }

        recyclerView.addOnScrollListener(object : RecyclerView.OnScrollListener() {
            override fun onScrolled(recyclerView: RecyclerView, dx: Int, dy: Int) {
                super.onScrolled(recyclerView, dx, dy)
                val lastVisible = layoutManager.findLastVisibleItemPosition()
                val itemCount = layoutManager.itemCount
                if (lastVisible >= itemCount - 3) viewModel.loadNextPage()
            }
        })

        viewModel.uiState.observe(this) { state -> render(state) }
        viewModel.load(append = false)
    }

    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        menuInflater.inflate(R.menu.menu_list, menu)
        val searchItem = menu.findItem(R.id.action_search)
        val searchView = searchItem?.actionView as? SearchView ?: return true
        searchView.setOnQueryTextListener(object : SearchView.OnQueryTextListener {
            override fun onQueryTextSubmit(query: String?): Boolean {
                applySearchQuery(query)
                return true
            }
            override fun onQueryTextChange(newText: String?): Boolean {
                searchRunnable?.let { searchHandler.removeCallbacks(it) }
                searchRunnable = Runnable { applySearchQuery(newText) }
                searchHandler.postDelayed(searchRunnable!!, searchDebounceMs)
                return true
            }
        })
        return true
    }

    private fun applySearchQuery(query: String?) {
        viewModel.setQuery(query?.trim()?.ifBlank { null })
        viewModel.load(append = false)
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        return when (item.itemId) {
            R.id.action_filter -> {
                showFilterSheet()
                true
            }
            R.id.action_new_game -> {
                openEdit(null)
                true
            }
            else -> super.onOptionsItemSelected(item)
        }
    }

    private fun showFilterSheet() {
        val sheet = FilterSortBottomSheetFragment.newInstance(
            viewModel.getCurrentPlatform(),
            viewModel.getCurrentStatus(),
            viewModel.getSortProp(),
            viewModel.getSortDir()
        )
        sheet.setCallback(object : FilterSortBottomSheetFragment.Callback {
            override fun onApply(platform: String?, status: String?, sortProp: String, sortDir: String) {
                viewModel.setFilters(platform, status)
                viewModel.setSort(sortProp, sortDir)
                viewModel.load(append = false)
            }
        })
        sheet.show(supportFragmentManager, "filter_sort")
    }

    private fun render(state: ListUiState?) {
        if (state == null) return
        progressBar.visibility = if (state.loading) View.VISIBLE else View.GONE
        val hasError = !state.error.isNullOrBlank()
        errorText.visibility = if (hasError) View.VISIBLE else View.GONE
        errorText.text = state.error
        retryButton.visibility = if (hasError) View.VISIBLE else View.GONE
        recyclerView.visibility = if (state.loading && state.items.isEmpty()) View.GONE else View.VISIBLE
        loadingMoreFooter.visibility = if (state.loadingMore) View.VISIBLE else View.GONE
        adapter.submitList(state.items)
    }

    private fun openEdit(gameId: String?) {
        val intent = Intent(this, EditActivity::class.java)
        if (gameId != null) intent.putExtra(EditActivity.EXTRA_GAME_ID, gameId)
        startActivity(intent)
    }

    override fun onResume() {
        super.onResume()
        viewModel.load(append = false)
    }
}
