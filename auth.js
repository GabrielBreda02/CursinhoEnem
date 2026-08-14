// ===============================
// Autenticação - funções compartilhadas por todas as páginas
// ===============================

const API_BASE = "http://localhost:5000/api";

const TOKEN_KEY = "bq_token";
const NOME_KEY = "bq_nome";
const EMAIL_KEY = "bq_email";
const TIPO_KEY = "bq_tipo";

function salvarSessao(token, nome, email, tipo) {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(NOME_KEY, nome);
    localStorage.setItem(EMAIL_KEY, email);
    localStorage.setItem(TIPO_KEY, tipo);
}

function getToken() {
    return localStorage.getItem(TOKEN_KEY);
}

function getNomeUsuario() {
    return localStorage.getItem(NOME_KEY);
}

function getTipoUsuario() {
    return localStorage.getItem(TIPO_KEY);
}

function ehProfessor() {
    return getTipoUsuario() === "Professor";
}

function ehAluno() {
    return getTipoUsuario() === "Aluno";
}

function estaLogado() {
    return !!getToken();
}

function logout() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(NOME_KEY);
    localStorage.removeItem(EMAIL_KEY);
    localStorage.removeItem(TIPO_KEY);
    window.location.href = "Login.html";
}

// fetch com o header Authorization já preenchido, para ações que exigem login.
// Se a API responder 401 (token ausente/expirado), limpa a sessão e manda para o login.
function authFetch(url, options = {}) {
    const token = getToken();
    const headers = { ...(options.headers || {}) };

    if (token) {
        headers["Authorization"] = `Bearer ${token}`;
    }

    return fetch(url, { ...options, headers }).then(response => {
        if (response.status === 401) {
            alert("Sua sessão expirou ou você não está logado. Faça login novamente.");
            logout();
            throw new Error("Não autenticado");
        }
        return response;
    });
}
