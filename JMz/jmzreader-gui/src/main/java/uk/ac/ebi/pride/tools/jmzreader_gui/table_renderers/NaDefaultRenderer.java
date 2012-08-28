package uk.ac.ebi.pride.tools.jmzreader_gui.table_renderers;

import java.awt.Component;
import java.text.DecimalFormat;

import javax.swing.JTable;
import javax.swing.table.DefaultTableCellRenderer;

/**
 * A defaut table cell renderer with the only
 * difference that NULL values are rendered
 * as "N/A".
 * @author jg
 *
 */
public class NaDefaultRenderer extends DefaultTableCellRenderer {
	private static final DecimalFormat format = new DecimalFormat("#.####");

	@Override
	public Component getTableCellRendererComponent(JTable table, Object value,
            boolean isSelected, boolean hasFocus,
            int row, int column) {
		
		if (value instanceof Double)
			value = format.format((Double) value);
		
		if (value == null)
			value = "N/A";
		
		return super.getTableCellRendererComponent(table, value, isSelected, hasFocus, row, column);
	}

}
