<style>
    .line
    {
        background-color: red;
        height: 1px;
    }
</style>

# <center>Informe Proyecto Capa de Enlace</center>
<center><h3>Equipo 15</h3>💬 Francisco Ayra Cáceres </br> 💬 Lázaro Daniel González </center>

![Imagen](Example.png)
## Características generales
- Seguiremos tratando el conjuntos de componentes anteriores en conexión como una misma componente conexa, y los Switches serían conexiones entre dichas componentes conexas.
- En caso de estar conectados dos switches, no habrían componentes conexas de por medio.
<div style = "height: 1px; background-color: #1C9997"></div>

**🔻 <font color = "1C9997">Los switches se comportan de la siguiente manera:</font>**
- En cada turno recepcionan los envios de cada puerto.
- Ponen dichos envios en cola.
- Existe una cola asociada a cada puerto.
- En caso de recibir un frame, si el switch sabe a que puerto enviarlo(sabe donde esta ubicada la `MAC` de destino, se pondría dicho frame solo en la cola de dicho respectivo puerto).
- En caso de un **broadcast** o en caso de no conocer a que puerto asociar la `MAC` de destino, pues el frame se pondría en todas las colas de los puertos.
- Si un switch detecta transmisión errónea, al igual que los Host's, posee un algoritmo de detección y verificación de errores, y si detecta que el frame esta sucio, intentara arreglarlo, si no se puede, no seguirá repitiendo el frame y este morirá ahí. Más adelante describiremos el algorimo de detección de errores.
<div style = "height: 1px; background-color: #1C9997"></div>

**🔻 <font color = "1C9997">Hay 4 formas de Transmisión vs Resultado en una misma componente conexa: </font>**
| | Dispositivos que transmiten| Resultado|
|--|-|-|
| 1 | <center>**Un host**</center>| Todos los Dispositivos en esa misma CC reciben el frame intacto.|
| 2 | <center>**Dos hosts**</center> | Cada Dispositivo en esa CC recive XOR del frame excepto los dos hosts transmisores, reciben el frame del otro|
| 3 | <center>**Un host y una señal externa de un switch**</center> | Dicho host recibe la señal externa del switch, el switch recibe la señal interna del host, los demás dispositivos en la CC reciben un XOR del frame.|
| 4 | <center>**Tres o más frames al unísono**</center> | Si se reciben 3 o mas señales al mismo tiempo, tanto de dispositivos internos, como de switches, cada dispositivo recibe un XOR de los valores de la transmisión de los n - 1 otros dispositivos(sea n el número de dispositivos transmitiendo en dicho CC + el número de switches conectados a esta CC y transmitiendo).|

<div style = "height: 1px; background-color: #1C9997"></div>

**🔻 <font color = "1C9997">Forma de tratar los datos en los switch</font>**
En caso de haber colisión en una CC, y esta este conectada a un switch, los datos estarán corruptos. El switch se encargará de detectar si el byte de verificación está correcto, y si está bien, actuará como **repetidor** de este frame comportándose de la forma que se comentó anteriormente.

<div style = "height: 1px; background-color: #1C9997"></div>

**🔻 <font color = "1C9997">Cambios en el Protocolo</font>**
Es sabido que en la capa física implementamos un protocolo para cada **host**, que se encargaba de detectar colisiones y en general era uno de los conocidos protocolos **ALOHA** con tiempo random de espera. Para detectar colisiones, cuando un dispositivo enviaba, se hallaba un XOR entre los valores que transmitían los dispositivos en una misma componente conexa, en cada componente conexa(CC); y luego, el método <font color = "C3D488"> Recive()</font> se invocaba en cada dispositivo, pasándole como parámetro el valor resultante de hacer XOR a los valores de los dispositivos  que estuviesen transmitiendo, de su componente conexa (incluyéndolo a él).
Pero en este caso, al tener el requerimiento de que un host pueda estar transmitiendo un bit y recibiendo otro sin que sea considerado una colisión, no se hara XOR entre todos los <font color = "1C9997">Dervice</font>'s de una misma CC, se hara XOR entre los n-1 valores de los n-1 otros dispositivos que esten transmitiendo en esa misma CC. Por tanto, nos será difícil simular este comportamento, al menos eficientemente, por lo que, para no obtener una complejidad muy significativa solamente para obtener que valor llegaría a cada host; donde, debemos almacenar que emite cada Host y usando técnicas dinámicas podemos calcular rápido eso (formará parte entonces del protocolo de los dispositivos). Se sigue mantendiendo el protocolo implementado anteriormente para los Hosts, un ALOHA no persistente(no vuelve a enviar hasta que no este libre el canal, o con un solo dispositivo transmitiendo) y con espera aleatoria de tiempo en caso de detectar una colisión.

<div style = "height: 1px; background-color: red"></div>

**🔻 <font color = "1C9997">Cosas a tener en cuenta en la ejecución</font>** 
Debido a la dificultad para simular el paso real de corriente por los cables y dispositivos, decidimos para una mejor simulación, regular de la siguiente manera, como estará conformada cada Componente Conexa:
- No existirá mas de un Hub en una misma CC. Esta regulación se debe a lo anteriormente dicho y también a que es lo mismo tener varios hub conectados entre si y a la vez conectando varios Host's, que tener 1 Hub conectando a todos estos Host's dichos.

<div style = "height: 1px; background-color: #1C9997"></div>

**🔻 <font color = "1C9997">Sobre la verificacion y correccion de errores</font>** 
Existen 2 formas conocidas para trabajar con **errores en frames**, verificar que existe error en un frame, y:
- Solicitar un reenvio del frame
- Intentar arreglarlo.

Decidimos implementar un algoritmo de Verificacion y Correccion de errores en frame. Este es conocido como Two-Dimensional-Parity Error Correction. Lo implementan tanto los switches como los Host's.

