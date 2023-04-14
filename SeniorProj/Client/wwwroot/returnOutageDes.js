let input = localStorage.getItem('input')

function returnText() {
    input = document.getElementById("outage_des").value
    localStorage.setItem('input', input)
    alert(input)
}

/* https://www.ceos3c.com/javascript/store-user-input-in-a-variable-with-javascript/ */