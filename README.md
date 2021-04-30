# Sloth Flying Website

เริ่มแรกในการเริ่มต้น Project ใช้คำสั่ง
```shell
$ dotnet restore
```

หลังจากนั้นถ้ายังไม่ติดตั้ง Entity Framework Core tools ใช้คำสั่ง
```shell
$ dotnet tool install --global dotnet-ef
```
เสร็จแล้วสามารถ Run เพื่อใช้งาน
```shell
$ dotnet watch run
```
Path ของ web application
```shell
wwwroot/
├─ Admin/
│  ├─ Blacklist
│  ├─ Login
│  └─ Logout
├─ Home/
│  ├─ Error
│  └─ Index
├─ Lab/
│  ├─ Booking{labId}
│  └─ Index
├─ LabAdmin/
│  ├─ EditItem{labId}
│  ├─ Index
│  └─ ViewItem{labId}
├─ Search/
│  ├─ Index
│  ├─ UserBooklist{userId}
│  └─ UserProfile{userId}
├─ User/
│  ├─ Booklist
│  ├─ EditProfile
│  ├─ Login
│  ├─ Logout
│  ├─ Profile
│  └─ Register
└─ Index
```
Path ของ api
```shell
wwwroot/
└─ Api/
   ├─ CancelBooking # ทำการยกเลิกเวลาที่จอง
   ├─ GetBooking # Get จำนวนของที่สามารถยืมได้ที่เหลือในแต่ละช่วงเวลา
   ├─ GetBookList # Get รายการที่ได้ทำการจองไว้
   └─ SetBooking # ทำการจองของ ณ ช่วงเวลานั้น
```
p.s. กรณีที่ pull มาใหม่ควรลบ database ที่มีอยู่เดิมไปก่อนเพราะ schema อาจจะเปลี่ยน
