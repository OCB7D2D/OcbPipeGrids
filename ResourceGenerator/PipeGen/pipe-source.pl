use strict;
use warnings;
use lib "lib";
use constant PI => 4 * atan2(1, 1);
use Math::Vector::Real;
use Meshes;
use Circle;
use Curve;
use Pipes;
use Cube;

my $details_circle = 6;
my $details_conic = 4;
my $cone_size = 1.75;
my $pipe_len = 0.2;
my $cone_len = 0.65;

my $M = Mesh->new;
my $T = Mesh->new;
my $B = Mesh->new;

my @rings;

my $axis = V(0, 1, 0);
my $norm = V(0, 0, 1);

my $height = $cone_len + $pipe_len;

# Use same radius as imported top and bottom convers use
my $circle = Circle->new(0.309, $details_circle, $axis, $norm);

# Move start position of circle
# Start from top at block pos 1
# Model may not touch the ground
$circle->MoveBy(V(0,0.5,0));

# Extrude the first flat pipe part
$circle = $circle->Extrude($M, 1);
# Move extruded copy down to create faces
$circle = $circle->MoveBy(- $axis * $pipe_len);

# Calculate factors to do smart stepping
my $fact_size = $cone_size ** (1 / $details_conic);
my $fact_len = $cone_len / log(1.0 + $details_conic);

# Create vector to apply to vector positions
my $scale_size = V($fact_size, 1, $fact_size);

for (my $i = 0; $i < $details_conic; $i++)
{
	$circle = $circle->Extrude($M, 1);
	my $move = (log(2 + $i) - log(1 + $i)) * $fact_len;
	$circle = $circle->MoveBy(- $axis * $move);
	$circle = $circle->Scale($scale_size);
}

$M->CylindricUVs($axis, $height, 2);
$M->InterpolateNormalsFromFaces();

my $lid_top = Mesh->read("import/pipe_straight/Mesh/N_P.obj")->ScaleBy(10000);
my $lid_bot = Mesh->read("import/pipe_straight/Mesh/S_P.obj")->ScaleBy(10000);

$lid_top->Rotate(PI/2,V(0,1,0),V(0,0.5,0.5))->MoveBy(V(0,0,-.5));
$lid_bot->Rotate(PI/2,V(0,1,0),V(0,0.5,0.5))->MoveBy(V(0,0,0.5 - $pipe_len - $cone_len));

$lid_top = $lid_top->Rotate(PI/2,V(-1,0,0),V(0,0,0));
$lid_bot = $lid_bot->Rotate(PI/2,V(-1,0,0),V(0,0,0));
$lid_bot = $lid_bot->Scale(V($cone_size,1,$cone_size));

$T->CopyFaces($lid_top);
$B->CopyFaces($lid_bot);

my $meshes = Meshes->new;
$meshes->AddMesh($M);
$meshes->AddMesh($T);
$meshes->AddMesh($B);

$M->WriteObj("pipe-source/M.obj", "M");
$T->WriteObj("pipe-source/T_P.obj", "T_P");
$B->WriteObj("pipe-source/B_P.obj", "B_P");

$meshes->WriteObj("pipe-source.obj", "PipePump");

my $box = $meshes->CalcBoundingBox();
print "Bounding Box ", $box->[0], " - ", $box->[1], "\n";

