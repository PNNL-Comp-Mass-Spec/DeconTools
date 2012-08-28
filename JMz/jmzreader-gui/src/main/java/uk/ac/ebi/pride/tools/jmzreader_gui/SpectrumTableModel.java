package uk.ac.ebi.pride.tools.jmzreader_gui;

import java.util.List;

import javax.swing.event.TableModelListener;
import javax.swing.table.TableModel;

public class SpectrumTableModel implements TableModel {
	private final String[] columnNames = {"Spectrum #", "Id", "MS Level", "Precursor m/z", "Precursor intensity", "Charge", "# of Peaks"};
	@SuppressWarnings("rawtypes")
	private final Class[] columnClasses = {Integer.class, String.class, Integer.class, Double.class, Double.class, Integer.class, Integer.class};

	private List<List<Object>> data;
	
	public SpectrumTableModel() {

	}
	
	

	public void addTableModelListener(TableModelListener arg0) {
		// TODO Auto-generated method stub

	}

	public Class<?> getColumnClass(int arg0) {
		return columnClasses[arg0];
	}

	public int getColumnCount() {
		return columnNames.length;
	}

	public String getColumnName(int arg0) {
		return columnNames[arg0];
	}

	public int getRowCount() {
		if (data == null)
			return 0;
		else
			return data.size();
	}

	public Object getValueAt(int arg0, int arg1) {
		// TODO: make sure the value exists
		return data.get(arg0).get(arg1);
	}

	public boolean isCellEditable(int arg0, int arg1) {
		// there are not editable cells...
		return false;
	}

	public void removeTableModelListener(TableModelListener arg0) {
		// TODO Auto-generated method stub

	}

	public void setValueAt(Object arg0, int arg1, int arg2) {
		// not possible

	}

	public void clearTable() {
		data = null;
	}
	
	public void setData(List<List<Object>> data) {
		this.data = data;
	}
	
	public void addData(List<List<Object>> data) {
		if (this.data == null)
			this.data = data;
		else
			this.data.addAll(data);
	}
}
