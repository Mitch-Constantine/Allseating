package com.allseating.android

import android.app.AlertDialog
import android.app.DatePickerDialog
import android.os.Bundle
import android.view.View
import android.widget.Button
import android.widget.ProgressBar
import android.widget.TextView
import android.widget.Toast
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import android.widget.EditText
import com.allseating.android.data.GameConstants
import com.allseating.android.ui.edit.EditUiState
import com.allseating.android.ui.edit.EditViewModel
import java.text.SimpleDateFormat
import java.util.Calendar
import java.util.Locale

class EditActivity : AppCompatActivity() {

    companion object {
        const val EXTRA_GAME_ID = "game_id"
    }

    private val gameId: String? by lazy { intent?.getStringExtra(EXTRA_GAME_ID) }

    private val viewModel: EditViewModel by viewModels {
        EditViewModelFactory((application as AllseatingApp).repository, (application as AllseatingApp).dateProvider, gameId)
    }

    private lateinit var progressBar: ProgressBar
    private lateinit var loadErrorText: TextView
    private lateinit var saveErrorText: TextView
    private lateinit var barcodeInput: EditText
    private lateinit var titleInput: EditText
    private lateinit var descriptionInput: EditText
    private lateinit var platformSpinner: android.widget.Spinner
    private lateinit var statusSpinner: android.widget.Spinner
    private lateinit var releaseDateInput: EditText
    private lateinit var priceInput: EditText
    private lateinit var reloadLatestButton: Button
    private lateinit var deleteButton: Button

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_edit)

        progressBar = findViewById(R.id.edit_progress)
        loadErrorText = findViewById(R.id.edit_load_error)
        saveErrorText = findViewById(R.id.edit_save_error)
        barcodeInput = findViewById(R.id.edit_barcode)
        titleInput = findViewById(R.id.edit_title)
        descriptionInput = findViewById(R.id.edit_description)
        platformSpinner = findViewById(R.id.edit_platform)
        statusSpinner = findViewById(R.id.edit_status)
        releaseDateInput = findViewById(R.id.edit_release_date)
        priceInput = findViewById(R.id.edit_price)
        reloadLatestButton = findViewById(R.id.edit_reload_latest)
        deleteButton = findViewById(R.id.edit_delete)

        setupSpinners()
        releaseDateInput.setOnClickListener { showDatePicker() }
        if (gameId != null) viewModel.loadGame(gameId!!)
        else supportActionBar?.title = "New Game"

        reloadLatestButton.visibility = if (gameId != null) View.VISIBLE else View.GONE
        reloadLatestButton.setOnClickListener { gameId?.let { viewModel.reloadLatest(it) } }
        deleteButton.visibility = if (gameId != null) View.VISIBLE else View.GONE
        deleteButton.setOnClickListener { confirmDelete() }
        findViewById<View>(R.id.edit_save).setOnClickListener { save() }

        viewModel.uiState.observe(this) { state -> render(state) }
    }

    private fun showDatePicker() {
        val cal = Calendar.getInstance()
        val raw = releaseDateInput.text?.toString()?.trim()
        if (!raw.isNullOrBlank()) {
            try {
                val fmt = SimpleDateFormat("yyyy-MM-dd", Locale.US)
                fmt.isLenient = true
                val parsed = fmt.parse(raw)
                if (parsed != null) {
                    cal.time = parsed
                }
            } catch (_: Exception) { /* use current cal */ }
        }
        DatePickerDialog(
            this,
            { _, year, month, dayOfMonth ->
                val c = Calendar.getInstance()
                c.set(Calendar.YEAR, year)
                c.set(Calendar.MONTH, month)
                c.set(Calendar.DAY_OF_MONTH, dayOfMonth)
                releaseDateInput.setText(SimpleDateFormat("yyyy-MM-dd", Locale.US).format(c.time))
            },
            cal.get(Calendar.YEAR),
            cal.get(Calendar.MONTH),
            cal.get(Calendar.DAY_OF_MONTH)
        ).show()
    }

    private fun setupSpinners() {
        val platformAdapter = android.widget.ArrayAdapter(this, android.R.layout.simple_spinner_item, GameConstants.PLATFORMS)
        platformAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item)
        platformSpinner.adapter = platformAdapter
        val statusAdapter = android.widget.ArrayAdapter(this, android.R.layout.simple_spinner_item, GameConstants.STATUSES)
        statusAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item)
        statusSpinner.adapter = statusAdapter
    }

    private fun render(state: EditUiState?) {
        if (state == null) return
        progressBar.visibility = if (state.loading) View.VISIBLE else View.GONE
        loadErrorText.visibility = if (!state.loadError.isNullOrBlank()) View.VISIBLE else View.GONE
        loadErrorText.text = state.loadError
        saveErrorText.visibility = if (!state.saveError.isNullOrBlank()) View.VISIBLE else View.GONE
        saveErrorText.text = state.saveError
        if (state.saveSuccess) {
            Toast.makeText(this, "Saved", Toast.LENGTH_SHORT).show()
            viewModel.clearSaveSuccess()
            window.decorView.post { finish() }
        }
        if (state.deleteSuccess) {
            Toast.makeText(this, "Deleted", Toast.LENGTH_SHORT).show()
            viewModel.clearDeleteSuccess()
            window.decorView.post { finish() }
        }
        if (state.showConcurrencyDialog) {
            viewModel.clearConcurrencyDialog()
            showConcurrencyDialog()
        }
        state.game?.let { game ->
            if (barcodeInput.text?.toString() != game.barcode) barcodeInput.setText(game.barcode)
            if (titleInput.text?.toString() != game.title) titleInput.setText(game.title)
            if (descriptionInput.text?.toString() != game.description) descriptionInput.setText(game.description)
            releaseDateInput.setText(game.releaseDate ?: "")
            priceInput.setText(String.format(Locale.US, "%.2f", game.price))
            val platformIdx = GameConstants.PLATFORMS.indexOf(game.platform)
            if (platformIdx >= 0) platformSpinner.setSelection(platformIdx)
            val statusIdx = GameConstants.STATUSES.indexOf(game.status)
            if (statusIdx >= 0) statusSpinner.setSelection(statusIdx)
        }
    }

    private fun save() {
        val barcode = barcodeInput.text?.toString() ?: ""
        val title = titleInput.text?.toString() ?: ""
        val description = descriptionInput.text?.toString() ?: ""
        val releaseDate = releaseDateInput.text?.toString()?.trim()?.take(10)?.ifBlank { null }
        val priceStr = priceInput.text?.toString() ?: "0"
        val price = priceStr.toDoubleOrNull() ?: 0.0
        val platform = GameConstants.PLATFORMS[platformSpinner.selectedItemPosition]
        val status = GameConstants.STATUSES[statusSpinner.selectedItemPosition]
        viewModel.save(gameId, barcode, title, description, platform, releaseDate, status, price)
    }

    private fun showConcurrencyDialog() {
        val id = gameId ?: return
        AlertDialog.Builder(this)
            .setTitle("This game was changed elsewhere")
            .setMessage("Another change was saved for this game. Reload the latest version to avoid overwriting it.")
            .setPositiveButton("Reload latest") { _, _ -> viewModel.reloadLatest(id) }
            .setNegativeButton(android.R.string.cancel, null)
            .show()
    }

    private fun confirmDelete() {
        val id = gameId ?: return
        AlertDialog.Builder(this)
            .setTitle("Delete game")
            .setMessage("Are you sure you want to delete this game?")
            .setPositiveButton(android.R.string.ok) { _, _ -> viewModel.delete(id) }
            .setNegativeButton(android.R.string.cancel, null)
            .show()
    }
}
