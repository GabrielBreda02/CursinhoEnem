// ===============================
// Script da Página de Login
// ===============================

const emailInput = document.getElementById("email");
const senhaInput = document.getElementById("senha");
const mensagemErro = document.getElementById("mensagemErro");
const avisoRegistro = document.getElementById("avisoRegistro");

if (new URLSearchParams(window.location.search).get("registrado") === "1") {
    avisoRegistro.style.display = "block";
}

function entrar() {
    mensagemErro.style.display = "none";

    const email = emailInput.value.trim();
    const senha = senhaInput.value;

    if (!email || !senha) {
        mensagemErro.textContent = "Preencha e-mail e senha.";
        mensagemErro.style.display = "block";
        return;
    }

    fetch(`${API_BASE}/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, senha })
    })
        .then(async response => {
            const dados = await response.json().catch(() => ({}));
            if (!response.ok) {
                throw new Error(dados.message || "E-mail ou senha inválidos");
            }
            return dados;
        })
        .then(dados => {
            salvarSessao(dados.token, dados.nome, dados.email, dados.tipo);
            window.location.href = "index.html";
        })
        .catch(erro => {
            mensagemErro.textContent = erro.message;
            mensagemErro.style.display = "block";
        });
}
