// ===============================
// Script da Página de Cadastro de Usuário
// ===============================

const nomeInput = document.getElementById("nome");
const emailInput = document.getElementById("email");
const senhaInput = document.getElementById("senha");
const confirmarSenhaInput = document.getElementById("confirmarSenha");
const mensagemErro = document.getElementById("mensagemErro");

function registrar() {
    mensagemErro.style.display = "none";

    const nome = nomeInput.value.trim();
    const email = emailInput.value.trim();
    const senha = senhaInput.value;
    const confirmarSenha = confirmarSenhaInput.value;

    if (!nome || !email || !senha || !confirmarSenha) {
        mostrarErro("Preencha todos os campos.");
        return;
    }

    if (senha.length < 6) {
        mostrarErro("A senha deve ter no mínimo 6 caracteres.");
        return;
    }

    if (senha !== confirmarSenha) {
        mostrarErro("As senhas não conferem.");
        return;
    }

    fetch(`${API_BASE}/auth/registrar`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ nome, email, senha })
    })
        .then(async response => {
            const dados = await response.json().catch(() => ({}));
            if (!response.ok) {
                throw new Error(dados.message || "Não foi possível concluir o cadastro.");
            }
            return dados;
        })
        .then(() => {
            window.location.href = "Login.html?registrado=1";
        })
        .catch(erro => mostrarErro(erro.message));
}

function mostrarErro(texto) {
    mensagemErro.textContent = texto;
    mensagemErro.style.display = "block";
}
