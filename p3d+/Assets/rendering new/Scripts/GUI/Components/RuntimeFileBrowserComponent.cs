﻿using System;
using System.IO;
using UnityEngine;

namespace UnityVolumeRendering
{
    public partial class RuntimeFileBrowser
    {
        /// <summary>
        /// Internal MonoBehaviour that shows the GUI of the file browser.
        /// </summary>
        public class RuntimeFileBrowserComponent : MonoBehaviour
        {
            public enum DialogMode
            {
                OpenFile,
                OpenDirectory,
                SaveFile
            }

            public DialogMode dialogMode = DialogMode.OpenFile;
            public DialogCallback callback = null;

            public string currentDirectory;
            private string selectedFile;
            private Vector2 scrollPos = Vector2.zero;

            private Rect windowRect = new Rect(100, 50, WINDOW_WIDTH, WINDOW_HEIGHT);

            private const int LEFT_PANEL_WIDTH = 100;
            private const int RIGHT_PANEL_WIDTH = 370;
            private const int WINDOW_WIDTH = 500;
            private const int WINDOW_HEIGHT = 300;

            private int windowID;

            private void Awake()
            {
                // Fetch a unique ID for our window (see GUI.Window)
                windowID = WindowGUID.GetUniqueWindowID();
            }

            private void OnGUI()
            {
                windowRect = GUI.Window(windowID, windowRect, UpdateWindow, "File browser");
            }

            private void UpdateWindow(int windowID)
            {
                GUI.DragWindow(new Rect(0, 0, 10000, 20));

                TextAnchor oldAlignment = GUI.skin.label.alignment;
                GUI.skin.button.alignment = TextAnchor.MiddleLeft;

                GUILayout.BeginVertical();

                DrawTopPanel();

                GUILayout.BeginHorizontal();

                DrawLeftSideMenu();

                DrawDirectoryView();

                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();

                DrawBottomBar();

                GUILayout.EndVertical();

                GUI.skin.button.alignment = oldAlignment;
            }

            private void DrawTopPanel()
            {
                GUILayout.BeginHorizontal();

                // "Back" button
                if (GUILayout.Button("Back", GUILayout.Width(LEFT_PANEL_WIDTH)))
                {
                    DirectoryInfo parentDir = Directory.GetParent(currentDirectory);
                    if (parentDir != null)
                        currentDirectory = parentDir.FullName;
                    else
                        currentDirectory = "";
                    scrollPos = Vector2.zero;
                }
                // Show current directory path
                currentDirectory = GUILayout.TextField(currentDirectory, GUILayout.Width(RIGHT_PANEL_WIDTH));

                GUILayout.EndHorizontal();
            }

            private void DrawLeftSideMenu()
            {
                GUILayout.BeginVertical(GUILayout.Width(LEFT_PANEL_WIDTH));

                foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
                {
                    if (GUILayout.Button(driveInfo.Name))
                    {
                        currentDirectory = driveInfo.Name;
                        scrollPos = Vector2.zero;
                    }
                }

                if (GUILayout.Button("Documents"))
                {
                    currentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    scrollPos = Vector2.zero;
                }
                if (GUILayout.Button("Desktop"))
                {
                    currentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    scrollPos = Vector2.zero;
                }

                GUILayout.EndVertical();
            }

            private void DrawDirectoryView()
            {
                GUILayout.BeginVertical();

                // Draw directory content
                if (!string.IsNullOrEmpty(currentDirectory) && Directory.Exists(currentDirectory))
                {
                    scrollPos = GUILayout.BeginScrollView(scrollPos);
                    // Draw directories
                    foreach (string dir in Directory.GetDirectories(currentDirectory))
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(dir);
                        if (GUILayout.Button(dirInfo.Name))
                        {
                            currentDirectory = dir;
                        }
                    }
                    // Draw files
                    if (dialogMode == DialogMode.OpenFile || dialogMode == DialogMode.SaveFile)
                    {
                        foreach (string file in Directory.GetFiles(currentDirectory))
                        {
                            FileInfo fileInfo = new FileInfo(file);
                            if (GUILayout.Button(fileInfo.Name))
                            {
                                selectedFile = fileInfo.FullName;
                            }
                        }
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();
            }

            private void DrawBottomBar()
            {
                GUILayout.BeginHorizontal();

                if(dialogMode == DialogMode.OpenFile || dialogMode == DialogMode.SaveFile)
                {
                    if (!string.IsNullOrEmpty(selectedFile))
                    {
                        FileInfo fileInfo = new FileInfo(selectedFile);
                        string fileName = Path.GetFileName(selectedFile);
                        // Show filename textbox
                        fileName = GUILayout.TextField(fileName, GUILayout.Width(RIGHT_PANEL_WIDTH));
                        selectedFile = Path.Combine(fileInfo.Directory.FullName, fileName);
                        GUILayout.FlexibleSpace();
                        // Show button
                        string buttonText = dialogMode == DialogMode.OpenFile ? "Open" : "Save";
                        if (File.Exists(selectedFile) && GUILayout.Button(buttonText))
                        {
                            CloseBrowser(false, selectedFile);
                        }
                    }
                }
                else if(dialogMode == DialogMode.OpenDirectory)
                {
                    if (!string.IsNullOrEmpty(currentDirectory))
                    {
                        // Show directory path textbox
                        currentDirectory = GUILayout.TextField(currentDirectory, GUILayout.Width(RIGHT_PANEL_WIDTH));
                        GUILayout.FlexibleSpace();
                        // Show button
                        string buttonText ="Open";
                        if (Directory.Exists(currentDirectory) && GUILayout.Button(buttonText))
                        {
                            CloseBrowser(false, currentDirectory);
                        }
                    }
                }

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Cancel"))
                {
                    CloseBrowser(true, "");
                }

                GUILayout.EndHorizontal();
            }

            private void CloseBrowser(bool cancelled, string selectedPath)
            {
                DialogResult result;
                result.cancelled = cancelled;
                result.path = selectedPath;

                callback?.Invoke(result);

                GameObject.Destroy(this.gameObject);
            }
        }
    }
}
