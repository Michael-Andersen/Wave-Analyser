#ifdef __cplusplus
#define EXPORT extern "C" __declspec (dllexport)
#else
#define EXPORT __declspec (dllexport)
#endif
#define IDC_RECORD_BEG                  1000
#define IDC_RECORD_END                  1001
#define IDC_PLAY_BEG                    1002
#define IDC_PLAY_PAUSE                  1003
#define IDC_PLAY_END                    1004
#define IDC_PLAY_REV                    1005
#define IDC_PLAY_REP                    1006
#define IDC_PLAY_SPEED                  1007
#define INP_BUFFER_SIZE 16384
// Next default values for new objects
// 
#ifdef APSTUDIO_INVOKED
#ifndef APSTUDIO_READONLY_SYMBOLS
#define _APS_NEXT_RESOURCE_VALUE        102
#define _APS_NEXT_COMMAND_VALUE         40001
#define _APS_NEXT_CONTROL_VALUE         1008
#define _APS_NEXT_SYMED_VALUE           101
#endif
#endif

EXPORT VOID CALLBACK Initialize();
EXPORT BOOL CALLBACK RecordStart();
EXPORT BOOL CALLBACK RecordStop();
EXPORT BOOL CALLBACK PlayStart();
EXPORT BOOL CALLBACK PlayStop();
EXPORT PBYTE CALLBACK Record_Proc(UINT message, WPARAM wParam, LPARAM lParam);
EXPORT void CALLBACK SetPSaveBuffer(PBYTE pSaveBuff);
EXPORT DWORD CALLBACK GetDWDataLength();
EXPORT BOOL CALLBACK PlayPause();
EXPORT VOID CALLBACK SetDWDataLength(DWORD dwdata);
EXPORT VOID CALLBACK SetChannels(WORD channels);
EXPORT VOID CALLBACK SetSampleRate(DWORD rate);
EXPORT VOID CALLBACK SetBitDepth(WORD depth);
EXPORT WORD CALLBACK GetChannels();
EXPORT WORD CALLBACK GetBitDepth();
EXPORT DWORD CALLBACK GetSampleRate();
EXPORT VOID CALLBACK SetHandle(HWND handle);
EXPORT VOID CALLBACK PlayPart();
EXPORT void CALLBACK SetPNewBuffer(PBYTE pSaveBuff);