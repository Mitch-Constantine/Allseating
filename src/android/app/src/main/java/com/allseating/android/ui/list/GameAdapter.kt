package com.allseating.android.ui.list

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import androidx.recyclerview.widget.DiffUtil
import androidx.recyclerview.widget.ListAdapter
import androidx.recyclerview.widget.RecyclerView
import com.allseating.android.R
import com.allseating.android.data.GameListItemDto
import java.util.Locale

class GameAdapter(
    private val onItemClick: (GameListItemDto) -> Unit
) : ListAdapter<GameListItemDto, GameAdapter.GameViewHolder>(DiffCallback) {

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): GameViewHolder {
        val view = LayoutInflater.from(parent.context).inflate(R.layout.item_game, parent, false)
        return GameViewHolder(view, onItemClick)
    }

    override fun onBindViewHolder(holder: GameViewHolder, position: Int) {
        holder.bind(getItem(position))
    }

    class GameViewHolder(
        itemView: View,
        private val onItemClick: (GameListItemDto) -> Unit
    ) : RecyclerView.ViewHolder(itemView) {
        private val title: TextView = itemView.findViewById(R.id.item_game_title)
        private val barcode: TextView = itemView.findViewById(R.id.item_game_barcode)
        private val platform: TextView = itemView.findViewById(R.id.item_game_platform)
        private val status: TextView = itemView.findViewById(R.id.item_game_status)
        private val price: TextView = itemView.findViewById(R.id.item_game_price)
        private val releaseDate: TextView = itemView.findViewById(R.id.item_game_release_date)

        fun bind(item: GameListItemDto) {
            title.text = item.title
            barcode.text = item.barcode
            platform.text = item.platform
            status.text = item.status
            price.text = String.format(Locale.US, "%.2f", item.price)
            releaseDate.text = item.releaseDate ?: "-"
            itemView.setOnClickListener { onItemClick(item) }
        }
    }

    private object DiffCallback : DiffUtil.ItemCallback<GameListItemDto>() {
        override fun areItemsTheSame(a: GameListItemDto, b: GameListItemDto) = a.id == b.id
        override fun areContentsTheSame(a: GameListItemDto, b: GameListItemDto) = a == b
    }
}
