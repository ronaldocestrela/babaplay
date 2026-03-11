# 📄 Documentação – Rotas de Associados

Este documento descreve os endpoints disponíveis no controller `AssociadosController`, incluindo métodos HTTP, permissões necessárias, parâmetros e possíveis respostas.

---

## 🔹 Base do Controller

- **Rota base:** `/api/associados` *(herdada de `BaseApiController`)*  
- **Requisitos gerais:**  
  - Autenticação via JWT
  - Autorização com atributos `ShouldHavePermission`
  - Todas as respostas retornam um objeto de resposta (`response`) com propriedade `IsSuccessful`.

---

### 1. **Criar Associado**

- **Método:** `POST`
- **Endpoint:** `/add`
- **Permissões:**  
  - Ação: `AssociationAction.Create`
  - Feature: `AssociationFeature.Associados`
- **Corpo da requisição:**  
  ```csharp
  CreateAssociadoRequest createAssociado
  ```
- **Campos (JSON):**
  ```json
  {
    "fullName": "string",                 // **obrigatório**
    "cpf": "string (11 dígitos)",         // **obrigatório**
    "dateOfBirth": "YYYY-MM-DD",         // **obrigatório** (deve ser no passado)
    "phoneNumber": "string",             // **obrigatório**
    "address": "string",                 // **obrigatório**
    "city": "string",                    // **obrigatório**
    "state": "string (2 caracteres)" ,   // **obrigatório**
    "zipCode": "string",                 // **obrigatório**
    "position": "string",                // **obrigatório**
    "email": "string",                   // **obrigatório**
    "password": "string",                // **obrigatório** (mín. 8 caracteres)
    "confirmPassword": "string"          // **obrigatório** (deve igualar senha)
  }
  ```
  > 📌 **Campos em negrito** são obrigatórios e possuem validações adicionais.

- **Retorno:**  
  - `200 OK` – quando `response.IsSuccessful`  
  - `400 Bad Request` – caso contrário

- **Exemplo de resposta bem-sucedida:**
  ```json
  {
    "isSuccessful": true,
    "data": "<novoAssociadoId>"
  }
  ```

- **Exemplo de erro (400):**
  ```json
  {
    "isSuccessful": false,
    "message": "Descrição do erro"
  }
  ```

---

### 2. **Atualizar Associado**

- **Método:** `PUT`
- **Endpoint:** `/update/{associadoId}`
- **Permissões:**  
  - Ação: `AssociationAction.Update`
  - Feature: `AssociationFeature.Associados`
- **Parâmetros de rota:**  
  - `associadoId` – string
- **Corpo da requisição:**  
  ```csharp
  UpdateAssociadoRequest updateAssociado
  ```
- **Campos (JSON):**
  ```json
  {
    "fullName": "string",                 // **obrigatório**
    "phoneNumber": "string",             // **obrigatório**
    "address": "string",                 // **obrigatório**
    "city": "string",                    // **obrigatório**
    "state": "string (2 caracteres)",    // **obrigatório**
    "zipCode": "string",                 // **obrigatório**
    "position": "string"                 // **obrigatório**
  }
  ```
  > 📌 Todos os campos acima são obrigatórios para atualização e seguem as mesmas regras de tamanho/formato do cadastro.

- **Retorno:**  
  - `200 OK` – quando `response.IsSuccessful`
  - `404 Not Found` – se não encontrado
  - `400 Bad Request` – erros de validação

- **Exemplo de resposta bem-sucedida:**
  ```json
  {
    "isSuccessful": true,
    "data": "<associadoId>",
    "message": "Associado atualizado com sucesso."
  }
  ```

- **Exemplo de erro (404):**
  ```json
  {
    "isSuccessful": false,
    "message": "Associado não encontrado."
  }
  ```

- **Exemplo de erro (400):**
  ```json
  {
    "isSuccessful": false,
    "message": "Erro de validação nos campos."
  }
  ```

---

### 3. **Excluir Associado**

- **Método:** `DELETE`
- **Endpoint:** `/{associadoId}`
- **Permissões:**  
  - Ação: `AssociationAction.Delete`
  - Feature: `AssociationFeature.Associados`
- **Parâmetros de rota:**  
  - `associadoId` – string
- **Retorno:**  
  - `200 OK` – quando `response.IsSuccessful`
  - `404 Not Found` – se não encontrado

- **Exemplo de sucesso (DELETE):**
  ```json
  {
    "isSuccessful": true,
    "message": "Associado removido com sucesso."
  }
  ```

- **Exemplo de erro (404):**
  ```json
  {
    "isSuccessful": false,
    "message": "Associado não encontrado."
  }
  ```

---

### 4. **Obter Associado por ID**

- **Método:** `GET`
- **Endpoint:** `/{associadoId}`
- **Permissões:**  
  - Ação: `AssociationAction.Read`
  - Feature: `AssociationFeature.Associados`
- **Parâmetros de rota:**  
  - `associadoId` – string
- **Retorno:**  
  - `200 OK` – quando `response.IsSuccessful`
  - `404 Not Found` – se não encontrado

- **Exemplo de sucesso (GET por ID):**
  ```json
  {
    "isSuccessful": true,
    "data": {
      "associadoId": "string",
      "fullName": "string",
      "cpf": "string",
      "dateOfBirth": "YYYY-MM-DD",
      "phoneNumber": "string",
      "address": "string",
      "city": "string",
      "state": "string",
      "zipCode": "string",
      "position": "string",
      "email": "string"
    }
  }
  ```

- **Exemplo de erro (404):**
  ```json
  {
    "isSuccessful": false,
    "message": "Associado não encontrado."
  }
  ```

---

### 5. **Listar Todos os Associados**

- **Método:** `GET`
- **Endpoint:** `/all`
- **Permissões:**  
  - Ação: `AssociationAction.Read`
  - Feature: `AssociationFeature.Associados`
- **Retorno:**  
  - `200 OK` – quando `response.IsSuccessful`
  - `404 Not Found` – se não encontrado

- **Exemplo de sucesso (lista):**
  ```json
  {
    "isSuccessful": true,
    "data": [
      {
        "associadoId": "string",
        "fullName": "string",
        "cpf": "string",
        "dateOfBirth": "YYYY-MM-DD",
        "phoneNumber": "string",
        "address": "string",
        "city": "string",
        "state": "string",
        "zipCode": "string",
        "position": "string",
        "email": "string"
      }
      // ... outros associados
    ]
  }
  ```

- **Exemplo de erro (404):**
  ```json
  {
    "isSuccessful": false,
    "message": "Nenhum associado encontrado."
  }
  ```

---

> 💡 **Observação:**  
Os comandos e queries utilizados no controller (`CreateAssociadoCommand`, `UpdateAssociadoCommand`, etc.) são definidos no projeto `Application.Features.Associados`.  
A validação de permissões utiliza constantes de `BabaPlayShared.Library.Constants`.

---

Essa documentação pode ser copiada para um arquivo `.md` no repositório ou integrada em um gerador de API.  
Se precisar de mais informações (ex.: exemplos de payloads ou schema dos requests), posso ajudar também!