# AI Customer Support Agent - Technical Architecture Report

## 1. High-Level Architecture
The AI Customer Support Agent is integrated into the **RentACar** solution using a **Layered Architecture** (Clean Architecture principles). It functions as a **Retrieval-Augmented Generation (RAG)** system where the AI is not just a chatbot, but an agent aware of the database state.

### System Flow
1.  **Customer** sends a message via Web UI.
2.  **Web Layer** receives request and passes it to Application Layer.
3.  **SupportManager** saves the message and triggers a background task.
4.  **Background Task** checks criteria (Is this a new query? Is an employee available?).
5.  **AiSupportContextManager** gathers data (User Profile, Bookings, Fleet Status) from **Core/Infrastructure**.
6.  **GeminiAgentService** sends context + user question to Google Gemini API.
7.  **AI Response** is saved to the database and broadcasted via **SignalR** to the UI.

---

## 2. Layer-by-Layer Breakdown

### Layer 1: Core (Domain Layer)
*Path: `RentACar.Core`*
This layer contains the fundamental business entities and contracts.

*   **Entities**:
    *   `SupportConversation`: Represents a ticket. Now includes `RequiresHumanIntervention` flag (boolean) to stop the AI from replying if the user requests a human.
    *   `SupportMessage`: Represents individual chats. Contains `SenderUserId` (which can be "AI_AGENT").
*   **Enums**:
    *   `SenderRole`: Defines who sent the message: `Customer`, `Employee`, or `System`.
*   **Interfaces**:
    *   `IGeminiAgentService`: Defines the contract for the AI service, allowing for easy mocking/testing.

### Layer 2: Infrastructure (Data Access Layer)
*Path: `RentACar.Infrastructure`*
Handles direct database interactions.

*   **Repositories**:
    *   `SupportConversationRepository`: Handles CRUD for tickets.
    *   `SupportMessageRepository`: Handles saving messages.
    *   `CategoryRepository`, `CarRepository`, `BookingRepository`: Used by the AI Context Manager to fetch raw data for the AI's "brain".
*   **Migrations**:
    *   Database schema updates were applied to support the new `RequiresHumanIntervention` column.

### Layer 3: Application (Business Logic Layer)
*Path: `RentACar.Application`*
This is where the "Brain" of the AI lives. It orchestrates everything.

#### A. Key Managers
1.  **`SupportManager.cs` ( The Orchestrator)**:
    *   **Method:** `SendMessageAsCustomerAsync`
    *   **Role:** When a customer messages, this manager captures it. Instead of waiting for a human, it spawns a `Task.Run` (fire-and-forget background task) to trigger the AI processing loop.
    *   **Logic:**
        *   Checks `RequiresHumanIntervention`. If true, AI stays silent.
        *   Checks if an employee has already replied. If yes, AI stays silent.
        *   If conditions met -> Call `GeminiAgentService`.

2.  **`AiSupportContextManager.cs` (The Data Collector / RAG)**:
    *   **Role:** Builds the "System Prompt" context.
    *   **Method:** `GetContextForCustomerAsync`
    *   **What it fetches:**
        *   **User Info:** Name, Verified Status, Phone, Email.
        *   **Active Booking:** Is the user currently renting a car? When is it due back?
        *   **Fleet Availability:** It queries `CarRepository` and `BookingRepository` to calculate exactly which cars are free for the next 45 days.
        *   **Policies:** Refund rules, deposit amounts.
    *   **Why:** This injected data prevents the AI from "hallucinating" (making up facts). It replies based on *your* real database.

#### B. Services
1.  **`GeminiAgentService.cs` (The AI Driver)**:
    *   **Role:** Talks to Google's Gemini-2.0-Flash API.
    *   **Logic:**
        *   Takes the massive text block created by the Context Manager.
        *   Takes the User's Message.
        *   Takes the last 6 messages of Conversation History (Memory).
        *   Sends all to Google.
        *   Receives the text response.

### Layer 4: Web (Presentation Layer)
*Path: `RentACar.Web`*
Handles user interaction and real-time updates.

*   **controllers**:
    *   `SupportController`: Standard MVC actions to load the chat page.
    *   `SupportApiController`: Provides the `/escalate` endpoint utilized by the "Speak to Human" button.
*   **SignalR Hub (`SupportChatHub`)**:
    *   Allows the AI's response (which is generated in a background thread) to instantaneously pop up on the user's screen without them refreshing the page.
*   **Configuration (`appsettings.json` / Env Vars)**:
    *   Stores the `GeminiChatbot:ApiKey` securely.

---

## 3. Detailed Workflow "Life of a Message"

1.  **User Types:** "Is the BMW X5 available for next week?"
2.  **Web Layer:** `SupportChatHub` receives the message.
3.  **App Layer (`SupportManager`):** Saves message to DB. Starts Background Worker.
4.  **App Layer (`AiSupportContextManager`):**
    *   Queries DB: "Does User X have a booking?" -> No.
    *   Queries DB: "Is BMW X5 active and unbooked next week?" -> Yes.
    *   Constructs Prompt: *"Context: User is verified. BMW X5 is available. Price is $100/day."*
5.  **App Layer (`GeminiAgentService`):**
    *   Sends to Google: *"System: [Context Above]. User: Is BMW X5 available?"*
    *   Google Replies: *"Yes, the BMW X5 is available for your dates at $100/day. Would you like to book it?"*
6.  **App Layer (`SupportManager`):**
    *   Saves AI reply to DB as "Sender: AI_AGENT".
    *   Calls `SignalRBroadcaster`.
7.  **Web Layer:** Logic pushes the detailed text to the user's browser via WebSocket.

## 4. Key Security & Safety Features
*   **Read-Only:** The AI is instructed via System Prompt that it **cannot** modify database records. It can only read.
*   **Escalation:** If the user clicks "Speak to Human", a flag is set in the DB, and the AI is hard-coded to ignore that conversation forevermore.
*   **Secrets:** API Keys are loaded via Environment Variables, keeping source code secure.
