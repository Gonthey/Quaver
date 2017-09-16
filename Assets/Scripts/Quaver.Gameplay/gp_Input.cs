﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Gameplay
{
    public partial class PlayScreen
    {
        private void input_CheckInput()
        {
            for (int k = 0; k < 4; k++)
            {
                if (!_keyDown[k])
                {
                    if (Input.GetKeyDown(_config_KeyBindings[k]))
                    {
                        skin_NoteDown(k);
                        input_JudgeNote(k + 1, _curSongTime);
                    }
                }
                else
                {
                    if (Input.GetKeyUp(_config_KeyBindings[k]))
                    {
                        skin_NoteUp(k);
                        input_JudgeLN(k + 1, _curSongTime);
                    }
                }
            }
        }

        // Check if LN is released on time or early
        private void input_JudgeLN(int kkey, float timePos)
        {
            int curNote = -1; //Cannot create null struct :(
            float closestTime = 1000f;
            for (int i = 0; i < _lnQueue.Count; i++)
            {
                if (_lnQueue[i].KeyLane == kkey)
                {
                    closestTime = timePos - _lnQueue[i].EndTime;
                    curNote = i;
                }
            }
            if (curNote >= 0 && curNote < _lnQueue.Count)
            {
                if (closestTime < -_judgeTimes[5])
                {
                    //Darkens early/mis-released LNs. Use skin images instead later

                    NoteObject newNote = new NoteObject();

                    newNote.StartTime = (int)_curSongTime;
                    newNote.EndTime = _lnQueue[curNote].EndTime;
                    newNote.KeyLane = _lnQueue[curNote].KeyLane;

                    newNote.HitSet = _lnQueue[curNote].HitSet;
                    newNote.HitSprite = _lnQueue[curNote].HitSprite;
                    newNote.SliderEndSprite = _lnQueue[curNote].SliderEndSprite;
                    newNote.SliderMiddleSprite = _lnQueue[curNote].SliderMiddleSprite;
                    newNote.SliderEndObject = _lnQueue[curNote].SliderEndObject;
                    newNote.SliderMiddleObject = _lnQueue[curNote].SliderMiddleObject;

                    newNote.HitSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    newNote.SliderMiddleSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    newNote.SliderEndSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);

                    _offLNQueue.Add(newNote);
                    _lnQueue.RemoveAt(curNote);
                    print("[Note Render] EARLY LN RELEASE");
                }
                else if (closestTime > -_judgeTimes[5] && closestTime < _judgeTimes[5])
                {
                    np_RemoveNote(_lnQueue[curNote].HitSet); ;
                    _lnQueue.RemoveAt(curNote);
                    print("[Note Render] PERFECT LN RELEASE");
                }
            }
        }
        
        //Check if note is hit on time/late/early
        private void input_JudgeNote(int kkey, float timePos)
        {
            if (_hitQueue[kkey - 1].Count > 0 && _hitQueue[kkey - 1][0].StartTime - _curSongTime < _judgeTimes[4])
            {
                float closestTime = _hitQueue[kkey - 1][0].StartTime - _curSongTime;
                float absTime = Mathf.Abs(closestTime);
                if (absTime < _judgeTimes[4])
                {
                    if (absTime < _judgeTimes[0])
                    {
                        print("[Note Render] MARV");
                    }
                    else if (absTime < _judgeTimes[1])
                    {
                        print("[Note Render] PERF");
                    }
                    else if (absTime < _judgeTimes[2])
                    {
                        print("[Note Render] GREAT");
                    }
                    else if (absTime < _judgeTimes[3])
                    {
                        print("[Note Render] GOOD");
                    }
                    else
                    {
                        print("[Note Render] BAD");
                    }
                    //Check if LN
                    if (_hitQueue[kkey - 1][0].EndTime > 0)
                    {
                        _lnQueue.Add(_hitQueue[kkey - 1][0]);
                        _hitQueue[kkey - 1].RemoveAt(0);
                    }
                    else
                    {
                        np_RemoveNote(_hitQueue[kkey - 1][0].HitSet);
                        _hitQueue[kkey - 1].RemoveAt(0);
                    }

                    skin_NoteBurst(kkey);
                }
            }
        }
    }
}