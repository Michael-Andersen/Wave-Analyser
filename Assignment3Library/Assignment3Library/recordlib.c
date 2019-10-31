

#include <windows.h>
#include "recordlib.h"

static BOOL         bRecording, bPlaying, bReverse, bPaused,
bEnding, bTerminating;
static DWORD        dwDataLength, dwRepetitions = 1;
static HWAVEIN      hWaveIn;
static HWAVEOUT     hWaveOut;
static PBYTE       pBuffer1, pBuffer2, pNewBuffer;
static PBYTE       pSaveBuffer;
static PWAVEHDR     pWaveHdr1, pWaveHdr2;
static TCHAR        szOpenError[] = TEXT("Error opening waveform audio!");
static TCHAR        szMemError[] = TEXT("Error allocating memory!");
static WAVEFORMATEX waveform;
static HWND hwnd;

TCHAR szAppName[] = TEXT("Record1");

int WINAPI DllMain(HINSTANCE hInstance, DWORD fdwReason, PVOID pvReserved)
{
	return TRUE;
}

EXPORT VOID CALLBACK SetHandle(HWND handle) {
	hwnd = handle;
}

EXPORT void CALLBACK SetPSaveBuffer(PBYTE pSaveBuff) {
	pSaveBuffer = pSaveBuff;
}

EXPORT DWORD CALLBACK GetDWDataLength() {
	return dwDataLength;
}

EXPORT BOOL CALLBACK RecordStop() {
	bEnding = TRUE;
	waveInReset(hWaveIn);
	return TRUE;
}

EXPORT VOID CALLBACK SetDWDataLength(DWORD dwdata) {
	dwDataLength = dwdata;
}

EXPORT VOID CALLBACK SetChannels(WORD channels) {
	waveform.nChannels = channels;
}

EXPORT WORD CALLBACK GetChannels() {
	return waveform.nChannels;
}

EXPORT VOID CALLBACK SetSampleRate(DWORD rate) {
	waveform.nSamplesPerSec = rate;
}

EXPORT DWORD CALLBACK GetSampleRate() {
	return waveform.nSamplesPerSec;
}

EXPORT WORD CALLBACK GetBitDepth() {
	return waveform.wBitsPerSample;
}

EXPORT VOID CALLBACK SetBitDepth(WORD depth) {
	waveform.wBitsPerSample = depth;
}


EXPORT BOOL CALLBACK PlayStart() {
	waveform.wFormatTag = WAVE_FORMAT_PCM;
	//waveform.nChannels = 1;
	//waveform.nSamplesPerSec = 11025;
	//waveform.wBitsPerSample = 8;
	waveform.nBlockAlign = waveform.nChannels * waveform.wBitsPerSample / 8;
	waveform.nAvgBytesPerSec = waveform.nSamplesPerSec * waveform.nBlockAlign;
	waveform.cbSize = 0;

	if (waveOutOpen(&hWaveOut, WAVE_MAPPER, &waveform,
		(DWORD)hwnd, 0, CALLBACK_WINDOW))
	{
		MessageBeep(MB_ICONEXCLAMATION);
		MessageBox(hwnd, szOpenError, szAppName,
			MB_ICONEXCLAMATION | MB_OK);
	}
	return TRUE;
}

EXPORT BOOL CALLBACK PlayStop() {

	bEnding = TRUE;
	waveOutReset(hWaveOut);
	return TRUE;
}

EXPORT void CALLBACK Initialize() {
	pWaveHdr1 = malloc(sizeof(WAVEHDR));
	pWaveHdr2 = malloc(sizeof(WAVEHDR));

	// Allocate memory for save buffer
	
	pSaveBuffer = malloc(1);
}

EXPORT BOOL CALLBACK RecordStart() {

	pBuffer1 = malloc(INP_BUFFER_SIZE);
	pBuffer2 = malloc(INP_BUFFER_SIZE);

	if (!pBuffer1 || !pBuffer2)
	{
		if (pBuffer1) free(pBuffer1);
		if (pBuffer2) free(pBuffer2);

		MessageBeep(MB_ICONEXCLAMATION);
		MessageBox(hwnd, szMemError, szAppName,
			MB_ICONEXCLAMATION | MB_OK);
		return TRUE;
	}

	// Open waveform audio for input

	waveform.wFormatTag = WAVE_FORMAT_PCM;
	//waveform.nChannels = 1;
	//waveform.nSamplesPerSec = 11025;
	waveform.nBlockAlign = waveform.nChannels * waveform.wBitsPerSample / 8;
	waveform.nAvgBytesPerSec = waveform.nSamplesPerSec * waveform.nBlockAlign;
	//waveform.wBitsPerSample = 8;
	waveform.cbSize = 0;

	if (waveInOpen(&hWaveIn, WAVE_MAPPER, &waveform,
		(DWORD)hwnd, 0, CALLBACK_WINDOW))
	{
		free(pBuffer1);
		free(pBuffer2);
		MessageBeep(MB_ICONEXCLAMATION);
		MessageBox(hwnd, szOpenError, szAppName,
			MB_ICONEXCLAMATION | MB_OK);
	}
	// Set up headers and prepare them

	pWaveHdr1->lpData = pBuffer1;
	pWaveHdr1->dwBufferLength = INP_BUFFER_SIZE;
	pWaveHdr1->dwBytesRecorded = 0;
	pWaveHdr1->dwUser = 0;
	pWaveHdr1->dwFlags = 0;
	pWaveHdr1->dwLoops = 1;
	pWaveHdr1->lpNext = NULL;
	pWaveHdr1->reserved = 0;

	waveInPrepareHeader(hWaveIn, pWaveHdr1, sizeof(WAVEHDR));

	pWaveHdr2->lpData = pBuffer2;
	pWaveHdr2->dwBufferLength = INP_BUFFER_SIZE;
	pWaveHdr2->dwBytesRecorded = 0;
	pWaveHdr2->dwUser = 0;
	pWaveHdr2->dwFlags = 0;
	pWaveHdr2->dwLoops = 1;
	pWaveHdr2->lpNext = NULL;
	pWaveHdr2->reserved = 0;

	waveInPrepareHeader(hWaveIn, pWaveHdr2, sizeof(WAVEHDR));
	return TRUE;
}

EXPORT BOOL CALLBACK PlayPause() {
	if (!bPaused)
	{
		waveOutPause(hWaveOut);
		SetDlgItemText(hwnd, IDC_PLAY_PAUSE, TEXT("Resume"));
		bPaused = TRUE;
	}
	else
	{
		waveOutRestart(hWaveOut);
		SetDlgItemText(hwnd, IDC_PLAY_PAUSE, TEXT("Pause"));
		bPaused = FALSE;
	}
	return TRUE;
}



EXPORT PBYTE CALLBACK Record_Proc(UINT message, WPARAM wParam, LPARAM lParam)
{
	

	switch (message)
	{
	case MM_WIM_OPEN:
		// Shrink down the save buffer

		pSaveBuffer = realloc(pSaveBuffer, 1);

		waveInAddBuffer(hWaveIn, pWaveHdr1, sizeof(WAVEHDR));
		waveInAddBuffer(hWaveIn, pWaveHdr2, sizeof(WAVEHDR));

		// Begin sampling

		bRecording = TRUE;
		bEnding = FALSE;
		dwDataLength = 0;
		waveInStart(hWaveIn);
		return NULL;

	case MM_WIM_DATA:

		// Reallocate save buffer memory

		pNewBuffer = realloc(pSaveBuffer, dwDataLength +
			((PWAVEHDR)lParam)->dwBytesRecorded);

		if (pNewBuffer == NULL)
		{
			waveInClose(hWaveIn);
			MessageBeep(MB_ICONEXCLAMATION);
			MessageBox(hwnd, szMemError, szAppName,
				MB_ICONEXCLAMATION | MB_OK);
			return NULL;
		}

		pSaveBuffer = pNewBuffer;
		CopyMemory(pSaveBuffer + dwDataLength, ((PWAVEHDR)lParam)->lpData,
			((PWAVEHDR)lParam)->dwBytesRecorded);

		dwDataLength += ((PWAVEHDR)lParam)->dwBytesRecorded;

		if (bEnding)
		{
			waveInClose(hWaveIn);
			return NULL;
		}

		// Send out a new buffer

		waveInAddBuffer(hWaveIn, (PWAVEHDR)lParam, sizeof(WAVEHDR));
		return NULL;

	case MM_WIM_CLOSE:
		// Free the buffer memory

		waveInUnprepareHeader(hWaveIn, pWaveHdr1, sizeof(WAVEHDR));
		waveInUnprepareHeader(hWaveIn, pWaveHdr2, sizeof(WAVEHDR));

		free(pBuffer1);
		free(pBuffer2);


		bRecording = FALSE;

		if (bTerminating)
			SendMessage(hwnd, WM_SYSCOMMAND, SC_CLOSE, 0L);

		
		return pSaveBuffer;

	case MM_WOM_OPEN:
		// Enable and disable buttons

		//EnableWindow(GetDlgItem(hwnd, IDC_RECORD_BEG), FALSE);
		//EnableWindow(GetDlgItem(hwnd, IDC_RECORD_END), FALSE);
		//EnableWindow(GetDlgItem(hwnd, IDC_PLAY_BEG), FALSE);
		//EnableWindow(GetDlgItem(hwnd, IDC_PLAY_PAUSE), TRUE);
		//EnableWindow(GetDlgItem(hwnd, IDC_PLAY_END), TRUE);
		//EnableWindow(GetDlgItem(hwnd, IDC_PLAY_REP), FALSE);
		//EnableWindow(GetDlgItem(hwnd, IDC_PLAY_REV), FALSE);
		//EnableWindow(GetDlgItem(hwnd, IDC_PLAY_SPEED), FALSE);
		//SetFocus(GetDlgItem(hwnd, IDC_PLAY_END));

		// Set up header

		pWaveHdr1->lpData = pSaveBuffer;
		pWaveHdr1->dwBufferLength = dwDataLength;
		pWaveHdr1->dwBytesRecorded = 0;
		pWaveHdr1->dwUser = 0;
		pWaveHdr1->dwFlags = WHDR_BEGINLOOP | WHDR_ENDLOOP;
		pWaveHdr1->dwLoops = dwRepetitions;
		pWaveHdr1->lpNext = NULL;
		pWaveHdr1->reserved = 0;

		// Prepare and write

		waveOutPrepareHeader(hWaveOut, pWaveHdr1, sizeof(WAVEHDR));
		waveOutWrite(hWaveOut, pWaveHdr1, sizeof(WAVEHDR));

		bEnding = FALSE;
		bPlaying = TRUE;
		return NULL;

	case MM_WOM_DONE:
		waveOutUnprepareHeader(hWaveOut, pWaveHdr1, sizeof(WAVEHDR));
		waveOutClose(hWaveOut);
		return NULL;

	case MM_WOM_CLOSE:
		// Enable and disable buttons

		//EnableWindow(GetDlgItem(hwnd, IDC_RECORD_BEG), TRUE);
		//EnableWindow(GetDlgItem(hwnd, IDC_RECORD_END), TRUE);
		//EnableWindow(GetDlgItem(hwnd, IDC_PLAY_BEG), TRUE);
		//EnableWindow(GetDlgItem(hwnd, IDC_PLAY_PAUSE), FALSE);
		//EnableWindow(GetDlgItem(hwnd, IDC_PLAY_END), FALSE);
		//EnableWindow(GetDlgItem(hwnd, IDC_PLAY_REV), TRUE);
		//EnableWindow(GetDlgItem(hwnd, IDC_PLAY_REP), TRUE);
		//EnableWindow(GetDlgItem(hwnd, IDC_PLAY_SPEED), TRUE);
		SetFocus(GetDlgItem(hwnd, IDC_PLAY_BEG));

		SetDlgItemText(hwnd, IDC_PLAY_PAUSE, TEXT("Pause"));
		bPaused = FALSE;
		dwRepetitions = 1;
		bPlaying = FALSE;

		
		if (bTerminating)
			SendMessage(hwnd, WM_SYSCOMMAND, SC_CLOSE, 0L);

		return pSaveBuffer;
	}
	return NULL;
}





