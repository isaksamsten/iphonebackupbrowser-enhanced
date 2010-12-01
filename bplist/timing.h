#pragma once

class timing
{
	LARGE_INTEGER start, freq;

public:
	timing()
	{
		QueryPerformanceFrequency(&freq);
		QueryPerformanceCounter(&start);
	}

	void restart()
	{
		QueryPerformanceCounter(&start);
	}

	double elapsed() const
	{
		LARGE_INTEGER end;
		QueryPerformanceCounter(&end);
		return (double)(end.QuadPart - start.QuadPart) / (double) freq.QuadPart;			
	}
};
