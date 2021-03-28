using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PixelArt {
    public class FileOpenScreen : UIScreen {

        public static FileOpenScreen instance;
        
        public FlexBox flex;
        public List<UIElement> tabs = new List<UIElement>();
        public List<UIElement> addTabs = new List<UIElement>();

        public float scroll, toScroll;
        public const float scrollAmount = 150, scrollSpeed = 20;

        public string dirPath;
        public UITextInput inputBar;

        public FileOpenScreen() {
            instance = this;
            
            flex = new FlexBox(tabs, genRect(), 30) {startType = FlexStartType.start};
            inputBar = new UITextInput(new Rectangle(50, 30, Main.screenWidth - 100, 50), tryPathChange) {enterOnly = true};
            
            changePath(Paths.texturePath);
            flex.apply();
            
            uiElements.Add(new PanelTop(new Rectangle(0, 0, Main.screenWidth, 110), 1));
            uiElements.Add(inputBar);
        }

        public void tryPathChange(string path) {
            if (Exporting.isValidPath(path)) {
                changePath(path);
            }
            else {
                inputBar.text = dirPath;
            }
        }

        public void changePath(string path) {
            dirPath = path;
            inputBar.text = path;
            toScroll = 0;
            
            Thread thread = new Thread(threadLoad);
            thread.Start();
        }

        public static void threadLoad() { 
            instance.loadFolder(instance.dirPath); // TODO: add locking to addTabs.add?
        }

        public void loadFolder(string dirPath) {
            foreach (var element in tabs) {
                element.delete = true;
            }
            tabs.Clear();
            
            string [] subDirs = Directory.GetDirectories(dirPath);
            string [] files = Directory.GetFiles(dirPath);
            
            int lastSlash = FileUtil.lastSlash(FileUtil.withoutTrailingSlash(dirPath));

            if (lastSlash != -1) { 
                addTabs.Add(new FileTab(dirPath.Substring(0, lastSlash), FileTabType.Folder) {image = Textures.get("FileTabBack")});
            }

            foreach(string subDir in subDirs)
                addTabs.Add(new FileTab(subDir, FileTabType.Folder));
            
            foreach (string file in files)
                if (file.ToLower().EndsWith(".png")) {
                    addTabs.Add(new FileTab(file, FileTabType.PNG));
                }
        }

        public void addTab(UIElement tab) {
            uiElements.Insert(tabs.Count, tab);
            tabs.Insert(tabs.Count, tab);
        }

        public static void open(FileTab tab) {
            switch (tab.fileType) {
                case FileTabType.PNG:
                    if (Main.latestKeys.shift) { 
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() {
                            FileName = FileUtil.dirIn(tab.path),
                            UseShellExecute = true,
                            Verb = "open"
                        });
                    }
                    else {
                        Main.addActiveProject(new Project(new Canvas(Textures.copy(tab.image))));
                        returnToMain();
                    }

                    break;
                case FileTabType.Folder:
                    if (Main.latestKeys.shift) { 
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() {
                            FileName = tab.path,
                            UseShellExecute = true,
                            Verb = "open"
                        });
                    }
                    else { 
                        instance.changePath(tab.path);
                    }
                    break;
            }
            
        }

        public Rectangle genRect() {
            return new Rectangle(50, 130 + (int) scroll, Main.screenWidth - 100, 100);
        }

        public override void update(MouseInfo mouse, KeyInfo keys, float deltaTime) {

            float lastScroll = scroll;

            toScroll -= mouse.scroll * scrollAmount;

            float bound = 0;
            if (tabs.Count > 0) {
                bound = tabs.Last().pos.Y - tabs.First().pos.Y;
            }

            toScroll += (Math.Clamp(toScroll, -bound, 0) - toScroll) * deltaTime * 30;
            
            scroll += (toScroll - scroll) * deltaTime * scrollSpeed;

            if (addTabs.Count > 0) {
                for (int i = 0; i < addTabs.Count; i++) {
                    addTab(addTabs[0]);
                    addTabs.RemoveAt(0);
                }
                flex.apply();
            }

            if (lastScroll != scroll) { 
                flex.rect = genRect();
                flex.apply();
            }

            base.update(mouse, keys, deltaTime);
        }

        public override void controls(float deltaTime, KeyInfo keys, MouseInfo mouse) {
            base.controls(deltaTime, keys, mouse);

            if (keys.pressed(Keys.M)) {
                returnToMain();
            }
        }
    }
}