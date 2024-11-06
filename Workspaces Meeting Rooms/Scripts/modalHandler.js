

//Globals
let isElementsAdded = false;

function startApp() {

    //let observeDOM = (() => {
    //    let MutationObserver = window.MutationObserver || window.WebkitMutationObserver,
    //        eventListenerSupported = window.addEventListener;

    //    return function (obj, callback) {
    //        if (MutationObserver) {
    //            let observ = new MutationObserver(function (mutations, observer) {
    //                mutations.forEach((mutation) => {
    //                    if (mutation.addedNodes.length || mutation.removedNodes.length) {
    //                        callback(observer)
    //                    }
    //                });
    //            });
    //            observ.observe(obj, { childList: true, subtree: true });
    //        } else if (eventListenerSupported) {
    //            obj.addEventListener("DOMNodeInserted", callback, false)
    //            obj.addEventListener("DOMNodeRemoved", callback, false)
    //        };
    //    };
    //})();

    //observeDOM(document.body, function () {


    //});

    captureModalOnEvent();

    const modal = document.querySelector("#roomModal");
    modal.addEventListener('hidden.bs.modal', () => {
        isElementsAdded = false;  // Reset the flag
        modal.removeAttribute("data-elements-loaded");  // Reset modal state
        // Limpiar el contenido anterior
        //document.querySelectorAll('[data-bs-toggle="modal"]').forEach((el) => el.innerHTML = "");
    });

}

//Muestra la ventana modal cuando se hace click en el boton de detalles
function captureModalOnEvent() {

    document.addEventListener("click", async (event) => {


        if (event.target.matches('[data-bs-toggle="modal"]')) {


            let roomId = event.target.getAttribute("data-room-id");

            const roomsDetailsbackendResponse = await getRoomDetails(roomId);
            const sessionBackendResponse = await GetSessionData();
            console.log(roomsDetailsbackendResponse);
            console.log(sessionBackendResponse);


            //Destructuring al objeto JSON para acceder a los valores necesitados (respuesta del servidor)
            let { roomEquipment, roomProperties } = roomsDetailsbackendResponse.roomData;
            let { userID } = sessionBackendResponse.userData;

            const reservationListbackendResponse = await GetReservationsList(roomId, userID);
            let { ReservationsListData } = reservationListbackendResponse;
            console.log(reservationListbackendResponse);

            //Uso de la tecnica asynchronous defer
            setTimeout(() => {
                getRoomAvailability();
            }, 0);

            //Si los elementos ya fueron agregados no se ejecutan las funciones en cascada abajo para evitar duplicaciones
            if (isElementsAdded && roomId == roomId) {
                return;
            }

            ShowReservationsArea(roomProperties.roomID, roomProperties.name, userID, 1);

            if (Array.isArray(roomEquipment)) {
                console.log("Array!!!")

                EquipmentListArea(roomEquipment);

            };

            ShowReservationsCreated(ReservationsListData);
    

        };
    });
};

async function getRoomAvailability() {
    const roomID = document.getElementById("roomID").value;
    const date = document.getElementById("startTime").value; // Suponiendo que este es el formato 'YYYY-MM-DD'
    const roomDetails = await getRoomDetails(roomID);
    let { roomData } = roomDetails;
    let { availability_start, availability_end } = roomData.roomProperties;

    console.log("Availability:", availability_start, "to", availability_end);

    // Crear un objeto Date en la zona horaria local para la fecha de hoy
    //const today = new Date();
    const todayStr = new Date().toISOString().split('T')[0]; // Correct format: YYYY-MM-DD
    console.log("Today is: " + todayStr);

    // Crear objetos Date para availability_start y availability_end en la zona horaria local
    const startAvailability = new Date(`${todayStr}T${availability_start}:00`);
    const endAvailability = new Date(`${todayStr}T${availability_end}:00`);

    console.log("The room availability start is: " + startAvailability.getTime());
    console.log("The room availability end is: " + endAvailability.getTime());


    const startTimeInput = document.getElementById("startTime");
    const endTimeInput = document.getElementById("endTime");

    // Establecer los límites de tiempo en formato local
    startTimeInput.min = startAvailability.toLocaleString('en-GB', { timeZone: 'UTC' }).slice(0, 16);
    endTimeInput.min = startAvailability.toLocaleString('en-GB', { timeZone: 'UTC' }).slice(0, 16);

    if (roomID && date) {
        const response = await fetch(`/Home/GetRoomAvailability?roomID=${roomID}&date=${date}`, {
            method: "GET",
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Network response was not ok: Body: ${response.statusText}, ${errorText}`);
        }

        const reservations = await response.json();
        console.log("Reservations:", reservations);

        // Convertir los horarios bloqueados de las reservas a objetos Date en formato local
        const blockedTimes = reservations.reservationsData.map(r => {
            const startTime = new Date(r.startTime); // Asegúrate de que este tiempo esté en UTC
            const endTime = new Date(r.endTime); // Igual aquí
            return {
                start: startTime.getTime(), // Convertir a milisegundos
                end: endTime.getTime() // Convertir a milisegundos
            };
        });

        console.log("Blocked times:", blockedTimes);

        // Función para verificar si el rango de tiempo seleccionado se superpone con algún bloque de tiempo reservado, retorna un true si se da la condicion
        function isTimeBlocked(selectedStartTime, selectedEndTime) {
            return blockedTimes.some(block => {
                console.log(`Checking block: ${new Date(block.start).toLocaleString()} to ${new Date(block.end).toLocaleString()} against selected: ${new Date(selectedStartTime).toLocaleString()} to ${new Date(selectedEndTime).toLocaleString()}`);
                return (selectedStartTime < block.end && selectedEndTime > block.start);
            });
        }

        function validateTimes() {
            const selectedStartTime = new Date(startTimeInput.value).getTime(); // Usar getTime para comparaciones
            console.log(selectedStartTime)
            const selectedEndTime = new Date(endTimeInput.value).getTime(); // Usar getTime para comparaciones

            if (isNaN(selectedStartTime) || isNaN(selectedEndTime)) {
                console.log("Invalid time selection");
                return false;
            }

            if (isTimeBlocked(selectedStartTime, selectedEndTime)) {
                alert("El tiempo seleccionado ya fue reservado por otro usuario. Por favor, elige otro momento donde haya disponibilidad");
                if (startTimeInput && endTimeInput) {
                    startTimeInput.value = ""; // Limpia el valor para forzar otra selección
                    endTimeInput.value = ""; // Limpia también el tiempo de fin
                
                    return false;
                }
            }

            console.log("selectedStartTime:", selectedStartTime);
            console.log("startAvailability:", startAvailability.getTime());
            console.log("endAvailability:", endAvailability.getTime());


            // Validación de inicio fuera de rango de disponibilidad
            if (selectedStartTime < startAvailability.getTime() || selectedStartTime >= endAvailability.getTime()) {
                alert("La hora de inicio seleccionada está fuera del rango de disponibilidad.");
                startTimeInput.value = ""; // Limpia para forzar otra selección
                return false;
            }

            // Validación de fin fuera de rango de disponibilidad
            if (selectedEndTime > endAvailability.getTime() || selectedEndTime <= startAvailability.getTime()) {
                alert("La hora de fin seleccionada está fuera del rango de disponibilidad.");
                endTimeInput.value = ""; // Limpia para forzar otra selección
                return false;
            }

            return true;
        }

        startTimeInput.addEventListener('change', validateTimes);
        endTimeInput.addEventListener('change', validateTimes);
    }
}


function EquipmentListArea(equipments) {
    const equipmentList = document.getElementById("equipmentList");

    //Se limpia la lista previamente para evitar datos pre cargados
    equipmentList.innerHTML = "";

    //Se valida si el parametro es un array
    if (Array.isArray(equipments)) {
        equipments.forEach((item) => {
            const listItem = document.createElement("li");
            listItem.textContent = item;
            equipmentList.appendChild(listItem);
            console.log("Items added!");
        });
    } else {
        console.error("La variable 'equipment' no es un array.");
    }

    //Garantiza que esta funcion se ejecute solo si no han sido insertado los elementos en el DOM
    isElementsAdded = true;
}

function ShowReservationsArea(roomID, roomName, userID, statusID) {

    //Generales
    const title = document.getElementById("roomTitleName");
    title.innerText = `Sala ${roomName}`;

    const mainColumn = document.querySelector("#displayColMain.col-md-4");

    if (document.querySelector("#reservationForm")) {
        return;
    }

    const div = document.createElement("div");
    div.classList.add = ("container");
    div.innerHTML = "";

    //Formulario
    const form = document.createElement('form');
    form.id = "reservationForm";
    form.method = 'POST';

    //Hidden inputs
    const roomIdHiddenInput = document.createElement("input");
    roomIdHiddenInput.name = "roomID";
    roomIdHiddenInput.id = "roomID";
    roomIdHiddenInput.type = "hidden";
    roomIdHiddenInput.value = roomID;

    //Hidden inputs
    const userIdHiddenInput = document.createElement("input");
    userIdHiddenInput.name = "userID";
    userIdHiddenInput.id = "userID";
    userIdHiddenInput.type = "hidden";
    userIdHiddenInput.value = userID;

    //Hidden inputs
    const statusIdHiddenInput = document.createElement("input");
    statusIdHiddenInput.name = "statusID";
    statusIdHiddenInput.id = "statusID";
    statusIdHiddenInput.type = "hidden";
    statusIdHiddenInput.value = statusID;

    //Entrada de tiempo/hora Inicio
    const divForInput1 = document.createElement("div");
    divForInput1.classList.add("mb-3");

    const labelStart = document.createElement("label");
    labelStart.classList.add("form-label");
    labelStart.textContent = "Hora de inicio";

    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0'); // Mes empieza en 0, por eso sumamos 1
    const day = String(now.getDate()).padStart(2, '0');
    const hours = String(now.getHours()).padStart(2, '0');
    const mins = String(now.getMinutes()).padStart(2, '0');

    const inputTimeDateStart = document.createElement("input");
    inputTimeDateStart.name = "startTime";
    inputTimeDateStart.id = "startTime";
    inputTimeDateStart.classList.add("form-control");
    inputTimeDateStart.type = "datetime-local";
    inputTimeDateStart.placeholder = "Seleccione la fecha y hora de inicio";
    inputTimeDateStart.value = `${year}-${month}-${day}T${hours}:${mins}`;
    //getRoomAvailability.call(inputTimeDateStart);


    //Entrada de tiempo/hora fin
    const divForInput2 = document.createElement("div");
    divForInput2.classList.add("mb-3");

    const labelEnd = document.createElement("label");
    labelEnd.classList.add("form-label");
    labelEnd.textContent = "Hora de terminacion"

    const inputTimeDateEnd = document.createElement("input");
    inputTimeDateEnd.name = "endTime";
    inputTimeDateEnd.id = "endTime";
    inputTimeDateEnd.classList.add("form-control");
    inputTimeDateEnd.type = "datetime-local";
    inputTimeDateEnd.placeholder = "Seleccione la fecha y hora de terminacion";


    if (inputTimeDateStart && inputTimeDateEnd) {
        modifyInputMinutesRanges(inputTimeDateStart);
        modifyInputMinutesRanges(inputTimeDateEnd);
    }


    //Boton para enviar la solicitud
    const submit = document.createElement("input");
    submit.type = "submit";
    submit.value = "Reservar"
    submit.id = "btnReservate"
    submit.classList.add("btn");
    submit.classList.add("btn-primary");

    //Anidacion de elementos
    if (mainColumn) {
        mainColumn.appendChild(div);
        div.appendChild(form);
        form.appendChild(roomIdHiddenInput);
        form.appendChild(userIdHiddenInput);
        form.appendChild(divForInput1);
        divForInput1.appendChild(labelStart);
        divForInput1.appendChild(inputTimeDateStart);
        form.appendChild(divForInput2);
        divForInput2.appendChild(labelEnd);
        divForInput2.appendChild(inputTimeDateEnd);
        form.appendChild(statusIdHiddenInput);
        form.appendChild(submit)
    }



    if (form) {

        form.addEventListener("submit", async (event) => {
            event.preventDefault();

            // Crear un objeto FormData para enviar los datos del formulario
            const formData = new FormData(form);
            const data = Object.fromEntries(formData); // Convertir FormData a objeto

            console.log('startTime:', data.startTime);
            console.log('endTime:', data.endTime);

            // Enviar la solicitud al servidor
            const response = await fetch('/Home/CreateReservation', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(data),
            });

            if (response.ok) {
                const result = await response.json();
                alert(result.message);
                const newReservation = await GetReservationsList(data.roomID, userID);
                ShowReservationsCreated(newReservation.ReservationsListData);
                if (inputTimeDateStart && inputTimeDateEnd) {
                    inputTimeDateStart.value = "";
                    inputTimeDateEnd.value = "";
                }
            } else {
                const errorText = await response.text();
                console.error(errorText);
            }
        });

    }
    //Garantiza que esta funcion se ejecute solo si no han sido insertado los elementos en el DOM
    isElementsAdded = true;
}

function modifyInputMinutesRanges(dateTimeInput) {
    dateTimeInput.addEventListener("input", () => {
        let date = new Date(dateTimeInput.value)

        if (!isNaN(date.getTime())) {
            const minutes = date.getMinutes();

            const roundedMinutes = minutes >= 30 ? 30 : 0;

            date.setMinutes(roundedMinutes);

            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            const hours = String(date.getHours()).padStart(2, '0');
            const mins = String(date.getMinutes()).padStart(2, '0');

            // Asigna el valor formateado
            dateTimeInput.value = `${year}-${month}-${day}T${hours}:${mins}`;
        } 

    });
}

function ShowReservationsCreated(reservations) {
   const container = document.getElementById("displaylReservations");
   container.innerHTML = ""; //Limpio el HTML del contenedor principal cuando le llama la funcion para rendizar cada cambio nuevo existente en el DOM desde el servidor

   const row = document.createElement("div");
   row.classList.add("row");
   row.classList.add("d-flex");
   row.classList.add("justify-content-between");

   const cont1 = document.createElement("div");
   cont1.classList.add("col-auto");
   const start = document.createElement("h6");
   start.textContent = "Inicio";

   const cont2 = document.createElement("div");
   cont2.classList.add("col-auto");
   const end = document.createElement("h6");
   end.textContent = "Fin";

   const cont3 = document.createElement("div");
   cont3.classList.add("col-auto");
   const statusTitle = document.createElement("h6");
   statusTitle.textContent = "Status";

   cont1.appendChild(start);
   cont2.appendChild(end);
   cont3.appendChild(statusTitle);

   row.appendChild(cont1);
   row.appendChild(cont2);
   row.appendChild(cont3);

   const listItemContainer = document.createElement("div");
   listItemContainer.classList.add("row");
   listItemContainer.classList.add("d-flex");
   listItemContainer.classList.add("justify-content-around");

   const startTimeItemContainer = document.createElement("div");
   startTimeItemContainer.classList.add("col-auto");

   const endTimeItemContainer = document.createElement("div");
   endTimeItemContainer.classList.add("col-auto");

   const statusItemContainer = document.createElement("div");
   statusItemContainer.classList.add("col-auto");

   reservations.forEach((item) => {

       const startTimeItem = document.createElement("p");
       startTimeItem.textContent = item.startTime;
       startTimeItemContainer.appendChild(startTimeItem);

       const endTimeItem = document.createElement("p");
       endTimeItem.textContent = item.endTime;
       endTimeItemContainer.appendChild(endTimeItem);

       const statusItem = document.createElement("p");
       statusItem.textContent = item.statusDescription;
       statusItemContainer.appendChild(statusItem);
   });


   listItemContainer.appendChild(startTimeItemContainer);
   listItemContainer.appendChild(endTimeItemContainer);
   listItemContainer.appendChild(statusItemContainer);

   container.appendChild(row);
   container.appendChild(listItemContainer);


   console.log("Reservations list added!");



   //Garantiza que esta funcion se ejecute solo si no han sido insertado los elementos en el DOM
   isElementsAdded = true;
}

async function GetSessionData() {
    //Peticion al controlador en el backend
    try {
        response = await fetch(`/Home/GetSessionData`,
                {
                method: "GET",
                headers: { "Accept": "application/json" }
            }
        );

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Network response was not ok: Body: ${response.statusText}, ${errorText}`);
        }

        //Lo convierto a JSON
        const data = await response.json();

        //Retorno los datos de la respuesta del servidor para usarlos eventualmene
        return data;

    } catch (error) {
        console.log("An error ocurred during the fetch request: ", error)
        return null;
    }
}

async function GetReservationsList(roomID, userID) {
        //Peticion al controlador en el backend
        try {
            const response = await fetch(`/Home/GetReservationsList?roomID=${roomID}&userID=${userID}`,
                {
                    method: "GET",
                    headers: { "Accept": "application/json" }
                }
            );

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Network response was not ok: Body: ${response.statusText}, ${errorText}`);
            }

            //Lo convierto a JSON
            const data = await response.json();

            //Retorno los datos de la respuesta del servidor para usarlos eventualmene
            return data;

        } catch (error) {
            console.log("An error ocurred during the fetch request: ", error)
            return null;
        }
    }

    async function getRoomDetails(roomID) {
        //Peticion al controlador en el backend
        try {
            const response = await fetch(`/Home/GetRoomDetails?id=${roomID}`,
                {
                    method: "GET",
                    headers: { "Accept": "application/json" }
                }
            );

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Network response was not ok: Body: ${response.statusText}, ${errorText}`);
            }

            //Lo convierto a JSON
            const data = await response.json();

            //Retorno los datos de la respuesta del servidor para usarlos eventualmene
            return data;

        } catch (error) {
            console.log("An error ocurred during the fetch request: ", error)
            return null;
        }
    }


window.onload = startApp;