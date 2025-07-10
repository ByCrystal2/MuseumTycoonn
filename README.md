**Regal Legacy**

**Overview** Regal Legacy is a 3D low‑poly museum simulation game built in Unity. Players explore a customizable museum environment, interact with exhibits, and curate their own collection of artifacts. Designed for  mobile  platforms, Regal Legacy leverages Firebase/Json authentication (email/password) and Unity's Universal Render Pipeline (URP) for high‑quality visuals and a smooth user experience.

**Key Features**

* **Immersive Exploration**: Wander through themed exhibition halls rendered in low‑poly style.
* **Customization System**: Customize your character's appearance, paintings on the wall and statue placement points in the room.
* **Interactive Artifacts**: Select individual items to view detailed descriptions and history.
* **Responsive UI**: Intuitive Unity UI panels for inventory, customization menus, and settings.
* **Cross‑Platform**: Supports Android, iOS
* **Secure Authentication:** Email/password login with Firebase Auth or Json; no need for third-party SDKs.
* **Modular Architecture**: Clean separation of core systems (PlayerController, UIManager).

**Demo & Screenshots**

> Trailer and Gameplay link: *[https://youtu.be/ZG99SPsPOZs](https://youtu.be/ZG99SPsPOZs)*
> Only Trailer link: [https://youtu.be/JHE-miDzkuk](https://youtu.be/JHE-miDzkuk*)

**Getting Started**

1. **Requirements**

   * Unity 2021.3 LTS or later
   * [.NET SDK 5.x or newer](https://dotnet.microsoft.com/)

2. **Clone the Repository**

```
git clone https://github.com/ByCrystal2/MuseumTycoonn.git
cd regal-legacy
```

3. **Install Dependencies**

   * Open the project in Unity Hub
   * In **Window > Package Manager**, ensure the following packages are installed:

     * Firebase Auth (optional)
     * Firebase Firestore (optional)
     * URP (Universal Render Pipeline)

4. **Configure Firebase (optional)**

   * In your Firebase Console, create a new project.
   * Enable **Email/Password** under **Authentication**.
   * Download `google-services.json` (Android) and `GoogleService-Info.plist` (iOS) and place them under `/Assets/StreamingAssets/`.

5. **Run & Build**

   * Press **Play** in the Unity Editor to test locally.
   * For Mobile: switch platform to Android or iOS and configure the SDK paths, then build normally.

**Gameplay Controls**

* **W/A/S/D (Only Editor) or Joystick: Character Movement Control**
* **UI Buttons:** Jump, dance, run, hit
* **Left‑Click/Touch**: Interact with exhibits
* **Escape / Back**: Open pause menu

**Contributing**
Contributions, issues, and feature requests are welcome! Please open a GitHub issue or submit a pull request for any improvements.

**License**
This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

*Thank you for checking out Regal Legacy! We hope you enjoy curating your own museum.*
 
