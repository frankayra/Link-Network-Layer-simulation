#<center>Informe Proyecto Capa de Enlace</center>
<center><h3>Equipo 15</h3>💬 Francisco Ayra Cáceres </br> 💬 Lázaro Daniel González </center>

![Imagen](Example.png)
## Caracteristicas generales
- Seguiremos tratando el conjuntos de componentes anteriores en conexion como una misma componente conexa, y los Switches serian conexiones entre dichas componentes conexas
- En caso de estar conectados dos switches, no habrian componentes conexas de por medio.
<div style = "height: 1px; background-color: #1C9997"></div>

**🔻 <font color = "1C9997">Los switches se comportan de la siguiente manera:</font>**
- En cada turno recepcionan los envios de cada puerto.
- Ponen dichos envios en cola.
- Existe una cola asociada a cada puerto.
- En caso de recibir un frame, si el switch sabe a que puerto enviarlo(sabe donde esta ubicada la `MAC` de destino, se pondria dicho frame solo en la cola de dicho respectivo puerto).
- En caso de un **broadcast** o en caso de no conocer a que puerto asociar la `MAC` de destino, pues el frame se pondria en todas las colas de los puertos.
<div style = "height: 1px; background-color: #1C9997"></div>

**🔻 <font color = "1C9997">Hay 4 formas de Transmision vs Resultado en una misma componente conexa: </font>**
| | Dispositivos que transmiten| Resultado|
|--|-|-|
| 1 | <center>**Un host**</center>| Todos los Dispositivos en esa misma CC reciben el frame intacto.|
| 2 | <center>**Dos hosts**</center> | Cada Dispositivo en esa CC recive XOR del frame excepto los dos hosts transmisores, reciben el frame del otro|
| 3 | <center>**Un host y una señal externa de un switch**</center> | Dicho host recibe la señal externa del switch, el switch recibe la señal interna del host, los demas dispositivos en la CC reciben un XOR del frame.|
| 4 | <center>**Tres o mas frames al unisono**</center> | Si se reciben 3 o mas señales al mismo tiempo, tanto de dispositivos internos, como de switches, cada dispositivo recibe un XOR de los valores de la transmision de los n - 1 otros dispositivos(sea n el numero de dispositivos transmitiendo en dicho CC + el numero de switches conectados a esta CC y transmitiendo).|

<div style = "height: 1px; background-color: #1C9997"></div>

**🔻 <font color = "1C9997">Forma de tratar los datos en los switch</font>**
En caso de haber colision en una CC, y esta este conectada a un switch, los datos estaran corruptos. El switch se encargara de detectar si el byte de verificacion esta correcto, y si esta bien, actuara como **repetidor** de este frame comportandose de la forma que se comento anteriormente.
<div style = "height: 1px; background-color: #1C9997"></div>

**🔻 <font color = "1C9997">Cambios en el Protocolo</font>**
Es sabido que en la capa fisica implementamos un protocolo para cada **host**, que se encargaba de detectar colisiones y en general era uno de los conocidos protocolos **ALOHA** con tiempo random de espera. Para detectar colisiones, cuando un dispositivo enviaba, se hallaba un XOR entre los valores que transmitian los dispositivos en una misma componente conexa, en cada componente conexa(CC); y luego, el metodo <font color = "C3D488"> Recive()</font> se invocaba en cada dispositivo, pasandole como parametro el valor resultante de hacer XOR a los valores de los dispositivos  que estuviesen transmitiendo, de su componente conexa.
Pero en este caso, al tener el requerimiento de que un host pueda estar transmitiendo un bit y reciviendo otro sin que sea considerado una colision, no se hara XOR entre todos los <font color = "1C9997">Dervice</font>'s de una misma CC, se hara XOR entre los n-1 valores de los n-1 otros dispositivos que esten transmitiendo en esa misma CC. Por tanto, nos sera dificil simular este comportamento, al menos eficientemente, por lo que, para no obtener una complejidad de **$\Theta(m * \frac{n(n-1))}{2})$** solamente para obtener que valor llegaria a cada host; donde **m** es la cantidad de conexiones en la CC y **n** es la cantidad de dispositivos transmitiendo en dicha CC; debemos almacenar que emite cada Host usando tecnicas de dinamismo para calcular rapido eso.
