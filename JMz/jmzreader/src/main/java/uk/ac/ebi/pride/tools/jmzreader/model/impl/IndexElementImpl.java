package uk.ac.ebi.pride.tools.jmzreader.model.impl;

import uk.ac.ebi.pride.tools.jmzreader.model.IndexElement;

/**
 * Simple implementation of the IndexElement
 * interface.
 * @author jg
 *
 */
public class IndexElementImpl implements IndexElement {
	private long start;
	private int size;
	
	public IndexElementImpl(long start, int size) {
		this.start = start;
		this.size = size;
	}

	@Override
	public long getStart() {
		return start;
	}

	@Override
	public int getSize() {
		return size;
	}

	public void setStart(long start) {
		this.start = start;
	}

	public void setSize(int size) {
		this.size = size;
	}
}
